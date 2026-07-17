using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Blade.MG.UI.Renderer;
using Blade.MG.UI.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI.Controls
{
    // Shared text-entry backing field for TextBox (full Material-style chrome: border, floating
    // label, helper text) and TextField (a lean underline-only field for places that don't want
    // that chrome - e.g. ComboBox's embedded editable header, or inline rename boxes). Holds the
    // caret/selection model and all the keyboard/mouse interaction (typing, cursor movement,
    // clipboard, click-to-position, drag-select) - everything a concrete subclass needs is just
    // its own visual template plus whatever extra chrome-specific properties it wants (Variant/
    // Label/HelperText/BorderColor live on TextBox, not here, since TextField has none of them).
    public abstract class TextEntryControl : TemplatedControl
    {
        private Binding<string> text = new Binding<string>();

        [DesignerProperty]
        public Binding<string> Text
        {
            get => text;
            set
            {
                // Adopt the assigned binding (e.g. a two-way binding to a view model property)
                // instead of only copying its current value into our own backing binding.
                SetField(ref text, value);

                if (text.Value != null && text.Value.Length > MaxLength)
                {
                    text.Value = text.Value.Substring(0, MaxLength);
                }

                ClampCursorAndSelection();
            }
        }

        public Binding<HorizontalAlignmentType> HorizontalTextAlignment;
        public Binding<VerticalAlignmentType> VerticalTextAlignment;

        private Binding<Color> textColor = new Binding<Color>();
        [DesignerProperty]
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        // Themed reactively (hover/focus - see TextBoxTemplate/TextFieldTemplate's own
        // HandleStateChange) rather than a fixed color, so the underline visibly indicates
        // focus the same way a full TextBox's border does. Overridable via SetStyleOverride
        // like every other themed property.
        private Binding<Color> underlineColor = new Binding<Color>();
        [DesignerProperty]
        public Binding<Color> UnderlineColor { get => underlineColor; set => SetField(ref underlineColor, value); }

        [DesignerProperty]
        public Binding<string> FontName { get; set; } = new Binding<string>();

        [DesignerProperty]
        public Binding<float> FontSize { get; set; } = new Binding<float>();

        [DesignerProperty]
        public bool WordWrap { get; set; }

        [DesignerProperty]
        public bool MultiLine { get; set; }

        [DesignerProperty]
        public int MaxLength { get; set; }

        public int CursorPosition { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }

        [DesignerProperty]
        public bool Underline { get; set; } // Underline the text

        // Anchor point for an in-progress selection (Shift+navigate or mouse drag).
        // Null means there's no active selection.
        private int? selectionAnchor;

        private bool isDragSelecting;

        protected bool HasSelection => SelectionLength > 0;

        protected TextEntryControl()
        {
            HorizontalTextAlignment = HorizontalAlignmentType.Left;
            VerticalTextAlignment = VerticalAlignmentType.Bottom;

            TextColor = UIManager.DefaultTheme.OnSurface;

            IsTabStop = true;
            IsHitTestVisible = true;

            FontName = null; // Use default
            FontSize = null; // Use default

            text = "";

            WordWrap = false;
            MultiLine = false;
            MaxLength = 250;

            CursorPosition = 0;
            SelectionStart = 0;
            SelectionLength = 0;

            Underline = true;
        }

        protected override void InitTemplate()
        {
            // Deliberately does NOT call base.InitTemplate() (TemplatedControl's own version) -
            // that would create and register a *second*, redundant template instance via
            // AddInternalChild purely so it participates in event propagation, which then also
            // gets rendered every frame (see UIComponentDrawable.RenderControl's InternalChildren
            // loop) right alongside the real one assigned to Content below. For a template whose
            // shape depends on per-frame Variant-driven state (TextBoxTemplate's CornerRadius/
            // Border), two independently-updated live copies risks exactly the kind of stray
            // rendering artifact this avoids (a duplicate "ghost" border briefly visible behind
            // the real one). TextEntryControl's own input handling never relies on that second
            // copy either - focus/keyboard/mouse are all handled directly on this control - so
            // skipping it is safe. TemplateInitialised itself is set by the Parent setter, not
            // here (see its own comment) - nothing left in the base implementation to lose.
            Content = Activator.CreateInstance(TemplateType) as UIComponent;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            MergeChildDesiredSize(context, ref availableSize, Content, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            if (Content != null)
            {
                Content.Arrange(context, FinalContentRect, FinalContentRect);
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, layoutBounds, parentTransform);
        }

        // ---=== Cursor / Selection model ===---

        protected string Value => Text.Value ?? "";

        private void ClampCursorAndSelection()
        {
            int length = Value.Length;

            CursorPosition = Math.Clamp(CursorPosition, 0, length);

            if (selectionAnchor != null)
            {
                selectionAnchor = Math.Clamp(selectionAnchor.Value, 0, length);
            }

            SelectionStart = Math.Clamp(SelectionStart, 0, length);
            SelectionLength = Math.Clamp(SelectionLength, 0, length - SelectionStart);
        }

        /// <summary>
        /// Moves the caret to <paramref name="position"/>. When <paramref name="extendSelection"/>
        /// is true (Shift held, or an in-progress drag-select), the selection is extended/created
        /// from wherever the caret was before this move; otherwise any selection is cleared.
        /// </summary>
        private void SetCursorPosition(int position, bool extendSelection)
        {
            position = Math.Clamp(position, 0, Value.Length);

            if (extendSelection)
            {
                selectionAnchor ??= CursorPosition;
            }
            else
            {
                selectionAnchor = null;
            }

            CursorPosition = position;
            UpdateSelectionFromAnchor();
        }

        private void UpdateSelectionFromAnchor()
        {
            if (selectionAnchor == null || selectionAnchor.Value == CursorPosition)
            {
                SelectionStart = CursorPosition;
                SelectionLength = 0;
            }
            else
            {
                SelectionStart = Math.Min(selectionAnchor.Value, CursorPosition);
                SelectionLength = Math.Abs(CursorPosition - selectionAnchor.Value);
            }
        }

        private void ClearSelection()
        {
            selectionAnchor = null;
            SelectionStart = CursorPosition;
            SelectionLength = 0;
        }

        /// <summary>
        /// Removes the selected text (if any) and collapses the caret to where it started.
        /// </summary>
        private void DeleteSelection()
        {
            if (!HasSelection)
            {
                return;
            }

            Text.Value = Value.Remove(SelectionStart, SelectionLength);
            CursorPosition = SelectionStart;
            ClearSelection();
        }

        private void MoveCursor(int direction, bool wordJump, bool extendSelection)
        {
            int newPosition;

            if (wordJump)
            {
                newPosition = direction < 0 ? FindPreviousWordBoundary(CursorPosition) : FindNextWordBoundary(CursorPosition);
            }
            else if (!extendSelection && HasSelection)
            {
                // Collapse an existing selection to its near edge, rather than moving the
                // caret by one character from wherever it happened to be - matches typical
                // desktop text-editing behavior.
                newPosition = direction < 0 ? SelectionStart : SelectionStart + SelectionLength;
            }
            else
            {
                newPosition = CursorPosition + direction;
            }

            SetCursorPosition(newPosition, extendSelection);
        }

        private int FindPreviousWordBoundary(int position)
        {
            string value = Value;
            int i = Math.Clamp(position, 0, value.Length);

            while (i > 0 && char.IsWhiteSpace(value[i - 1])) i--;
            while (i > 0 && !char.IsWhiteSpace(value[i - 1])) i--;

            return i;
        }

        private int FindNextWordBoundary(int position)
        {
            string value = Value;
            int i = Math.Clamp(position, 0, value.Length);

            while (i < value.Length && char.IsWhiteSpace(value[i])) i++;
            while (i < value.Length && !char.IsWhiteSpace(value[i])) i++;

            return i;
        }

        // ---=== UI Events ===---

        public override Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            if (uiEvent.Focused)
            {
                // Default the caret to the end of the existing text when focus is first gained,
                // rather than leaving it wherever it last was (position 0, for a box that's
                // never been clicked into before). A subsequent click while already focused
                // still positions the caret precisely as normal - see
                // HandleMouseDownEventAsync/GetCharacterIndexAtX - this only covers the initial
                // focus transition itself (click-to-focus, Tab, or a composite control like
                // ComboBox focusing its embedded EditBox).
                SetCursorPosition(Value.Length, false);
            }

            return base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            // Handle keyboard input if this control has focus (matches CheckBox.ActivateAsync's
            // own IsEnabled guard - a disabled field can still incidentally hold focus/hover
            // since neither is blocked at the hit-test level, but shouldn't accept edits).
            if (!uiEvent.Handled && HasFocus.Value && IsEnabled.Value)
            {
                await HandleKeyAsync(uiWindow, uiEvent);
            }

            // Propagate to children
            await base.HandleKeyPressAsync(uiWindow, uiEvent);
        }

        private async Task HandleKeyAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            // Enter commits the edit the same way clicking away does: give up focus, which
            // triggers whatever the app wired up to react to losing focus (e.g. a rename box
            // committing its new value on HandleFocusChangedEventAsync). Only for single-line
            // boxes - a MultiLine box should get a literal newline instead.
            if (!MultiLine && uiEvent.Key == Keys.Enter)
            {
                uiEvent.Handled = true;

                if (uiWindow != null)
                {
                    await uiWindow.SetFocusAsync(null);
                }

                return;
            }

            // Tab also commits the edit (same as clicking away), but - unlike Enter - moves on
            // to the next/previous tab stop instead of dropping focus entirely, matching
            // standard Tab-between-fields behavior. Without this, tabbing into any text field was
            // a dead end: the previous "just defocus" behavior consumed the keypress before
            // UIManager.Keyboard.cs's own Tab handling ever saw it, so navigation could never
            // continue past a text field.
            if (uiEvent.Key == Keys.Tab)
            {
                uiEvent.Handled = true;

                if (uiWindow != null)
                {
                    if (uiEvent.Shift)
                    {
                        await uiWindow.HandleTabPrevious();
                    }
                    else
                    {
                        await uiWindow.HandleTabNext();
                    }
                }

                return;
            }

            // Clipboard / select-all shortcuts. Checked before KeyChar, since GetChar() maps
            // letter keys to a character regardless of whether Ctrl is also held.
            if (uiEvent.Ctrl)
            {
                switch (uiEvent.Key)
                {
                    case Keys.C:
                        CopySelection();
                        uiEvent.Handled = true;
                        return;

                    case Keys.X:
                        CutSelection();
                        uiEvent.Handled = true;
                        return;

                    case Keys.V:
                        PasteClipboard();
                        uiEvent.Handled = true;
                        return;

                    case Keys.A:
                        SelectAll();
                        uiEvent.Handled = true;
                        return;
                }
            }

            if (uiEvent.KeyChar != null)
            {
                AddChar(uiEvent.KeyChar);
                uiEvent.Handled = true;
                return;
            }

            switch (uiEvent.Key)
            {
                case Keys.Back:
                    HandleBackspace();
                    uiEvent.Handled = true;
                    break;

                case Keys.Delete:
                    HandleDelete();
                    uiEvent.Handled = true;
                    break;

                case Keys.Left:
                    MoveCursor(-1, uiEvent.Ctrl, uiEvent.Shift);
                    uiEvent.Handled = true;
                    break;

                case Keys.Right:
                    MoveCursor(1, uiEvent.Ctrl, uiEvent.Shift);
                    uiEvent.Handled = true;
                    break;

                case Keys.Home:
                    SetCursorPosition(0, uiEvent.Shift);
                    uiEvent.Handled = true;
                    break;

                case Keys.End:
                    SetCursorPosition(Value.Length, uiEvent.Shift);
                    uiEvent.Handled = true;
                    break;
            }
        }

        private void AddChar(string character)
        {
            string value = Value;

            if (HasSelection)
            {
                value = value.Remove(SelectionStart, SelectionLength);
                CursorPosition = SelectionStart;
                ClearSelection();
            }

            if (value.Length >= MaxLength)
            {
                return;
            }

            Text.Value = value.Insert(CursorPosition, character);
            CursorPosition += character.Length;
            ClearSelection();
        }

        private void HandleBackspace()
        {
            if (HasSelection)
            {
                DeleteSelection();
                return;
            }

            if (CursorPosition <= 0)
            {
                return;
            }

            Text.Value = Value.Remove(CursorPosition - 1, 1);
            CursorPosition -= 1;
            ClearSelection();
        }

        private void HandleDelete()
        {
            if (HasSelection)
            {
                DeleteSelection();
                return;
            }

            string value = Value;
            if (CursorPosition >= value.Length)
            {
                return;
            }

            Text.Value = value.Remove(CursorPosition, 1);
            ClearSelection();
        }

        private void CopySelection()
        {
            if (!HasSelection)
            {
                return;
            }

            ClipboardService.SetText(Value.Substring(SelectionStart, SelectionLength));
        }

        private void CutSelection()
        {
            if (!HasSelection)
            {
                return;
            }

            CopySelection();
            DeleteSelection();
        }

        private void PasteClipboard()
        {
            string clip = ClipboardService.GetText();
            if (string.IsNullOrEmpty(clip))
            {
                return;
            }

            // This is a single-line text box - strip any newlines from the pasted text.
            clip = clip.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');

            string value = Value;

            if (HasSelection)
            {
                value = value.Remove(SelectionStart, SelectionLength);
                CursorPosition = SelectionStart;
                ClearSelection();
            }

            int availableLength = MaxLength - value.Length;
            if (availableLength <= 0)
            {
                return;
            }

            if (clip.Length > availableLength)
            {
                clip = clip.Substring(0, availableLength);
            }

            Text.Value = value.Insert(CursorPosition, clip);
            CursorPosition += clip.Length;
            ClearSelection();
        }

        private void SelectAll()
        {
            selectionAnchor = 0;
            CursorPosition = Value.Length;
            UpdateSelectionFromAnchor();
        }

        // ---=== Mouse: click to position caret, drag to select ===---

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (!uiEvent.Handled && IsEnabled.Value && uiEvent.PrimaryButton.Pressed && ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)))
            {
                int index = (Content as ITextEntryTemplate)?.GetCharacterIndexAtX(uiEvent.X) ?? Value.Length;
                SetCursorPosition(index, false);

                isDragSelecting = true;
                LockEventsToControl(uiWindow, this);

                uiEvent.Handled = true;
            }

            await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent)
        {
            if (isDragSelecting)
            {
                int index = (Content as ITextEntryTemplate)?.GetCharacterIndexAtX(uiEvent.X) ?? CursorPosition;
                SetCursorPosition(index, true);
            }

            await base.HandleMouseMoveEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (isDragSelecting)
            {
                isDragSelecting = false;
                UnlockEventsFromControl(uiWindow, this);
                uiEvent.Handled = true;
            }

            await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
        }
    }
}
