using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Blade.MG.UI.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI.Controls
{
    public class TextBox : TemplatedControl
    {
        private Binding<string> text = new Binding<string>();

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
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        private Binding<Color> borderColor = new Binding<Color>();
        public Binding<Color> BorderColor { get => borderColor; set => SetField(ref borderColor, value); }

        public Binding<string> FontName { get; set; } = new Binding<string>();

        public Binding<float> FontSize { get; set; } = new Binding<float>();

        public bool WordWrap { get; set; }

        public bool MultiLine { get; set; }

        public int MaxLength { get; set; }

        public int CursorPosition { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }

        public Variant Variant { get; set; }
        public string Label { get; set; }
        public string HelperText { get; set; }
        public bool Underline { get; set; } // Underline the text
        public bool ShrinkLabel { get; set; } // Label stays Shrunk and doesn't fill the textbox if the text is empty

        // Anchor point for an in-progress selection (Shift+navigate or mouse drag).
        // Null means there's no active selection.
        private int? selectionAnchor;

        private bool isDragSelecting;

        private bool HasSelection => SelectionLength > 0;


        public TextBox()
        {
            TemplateType = typeof(TextBoxTemplate);

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

            Variant = Variant.Standard;
            Label = null;
            HelperText = null;

            //MinHeight = 30;

            Underline = true;
            ShrinkLabel = false;

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

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

        private string Value => Text.Value ?? "";

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
            return base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            // Handle keyboard input if this control has focus
            if (!uiEvent.Handled && HasFocus.Value)
            {
                await HandleKeyAsync(uiWindow, uiEvent);
            }

            // Propagate to children
            await base.HandleKeyPressAsync(uiWindow, uiEvent);
        }

        private async Task HandleKeyAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            // Enter/Tab commit the edit the same way clicking away does: give up focus,
            // which triggers whatever the app wired up to react to losing focus (e.g. a
            // rename box committing its new value on HandleFocusChangedEventAsync). Only
            // for single-line boxes - a MultiLine box should get a literal newline instead.
            if (!MultiLine && (uiEvent.Key == Keys.Enter || uiEvent.Key == Keys.Tab))
            {
                uiEvent.Handled = true;

                if (uiWindow != null)
                {
                    await uiWindow.SetFocusAsync(null);
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
            if (!uiEvent.Handled && uiEvent.PrimaryButton.Pressed && FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                int index = (Content as TextBoxTemplate)?.GetCharacterIndexAtX(uiEvent.X) ?? Value.Length;
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
                int index = (Content as TextBoxTemplate)?.GetCharacterIndexAtX(uiEvent.X) ?? CursorPosition;
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
