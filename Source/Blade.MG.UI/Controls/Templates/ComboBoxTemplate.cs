using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls.Templates;

public class ComboBoxTemplate : Control
{
    public Border RootBorder;
    public Label DisplayLabel;
    public Border HeaderBorder;
    public Panel HeaderPanel;
    public IconButton DropDownButton;
    public Rectangle HeaderRect => HeaderBorder?.FinalRect ?? FinalRect;
    public ListView ListView;
    public TextField EditBox;

    // Tracks EditBox.HasFocus across frames so the dropdown auto-opens exactly once when focus
    // is gained (see Arrange), rather than re-opening every frame it happens to be both focused
    // and closed - which would fight the user right back open after they close it with Escape
    // (Escape closes the dropdown without dropping EditBox's focus - see
    // ComboBox.HandleKeyPressAsync).
    private bool wasEditBoxFocused;

    protected override void InitTemplate()
    {
        base.InitTemplate();

        var combo = ParentAs<ComboBox>();

        // Root border contains header and the list
        RootBorder = new Border()
        {
            // Link to the ComboBox's own BorderColor binding so a developer can override it
            // directly (combo.BorderColor = ...) as well as via SetStyleOverride.
            BorderColor = combo.BorderColor,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Top,
            Padding = new Thickness(4)
        };

        // Header (shows selected text or an editable TextField)
        HeaderBorder = new Border()
        {
            BorderColor = Color.Transparent,
            BorderThickness = new Thickness(0),
            Background = Color.Transparent,
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Top,
            Padding = new Thickness(6, 4, 6, 4)
        };

        // Label for non-editable state
        DisplayLabel = new Label()
        {
            Text = combo.Text, // binding in render
            Background = Color.Transparent,

            // Link to the ComboBox's own TextColor binding so a developer can override it
            // directly (combo.TextColor = ...) as well as via SetStyleOverride.
            TextColor = combo.TextColor,
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Stretch,
            Margin = new Thickness(0, 0, 24, 0) // leave room for DropDownButton
        };

        // Lean text field for editable state - just the text and an underline, none of
        // TextBox's border/floating-label/helper-text chrome, which is both unneeded here and
        // was previously what forced the combo box's minimum header height up (see
        // ComponentTesterUI's Height = 40 comment history). Its themed background already
        // defaults to transparent (see TextFieldTemplate), so the combo box's own hover/focus
        // background (applied to RootBorder - see HandleStateChange) shows through without
        // needing a style override.
        EditBox = new TextField()
        {
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Stretch,
            Margin = new Thickness(0, 0, 24, 0), // leave room for DropDownButton
            MaxLength = 250
        };

        // Dropdown toggle button - drawn as an up/down chevron depending on IsDropDownOpen,
        // reusing the same "Images/arrow_up_small" texture ScrollBar.cs already draws its
        // endcap arrows with, flipped vertically for the closed (down) state.
        DropDownButton = new IconButton()
        {
            Width = 24,
            Height = 24,
            HorizontalAlignment = HorizontalAlignmentType.Right,
            VerticalAlignment = VerticalAlignmentType.Stretch,
            IconSize = 12,
            DrawIcon = (ctx, spriteBatch, rect, color) =>
            {
                var arrowTexture = ctx.LoadContent<Texture2D>("Images/arrow_up_small");
                var effects = combo.IsDropDownOpen ? SpriteEffects.None : SpriteEffects.FlipVertically;
                spriteBatch.Draw(arrowTexture, rect, null, color, 0f, Vector2.Zero, effects, 0f);
            },
            OnActivate = (sender, e) =>
            {
                combo.IsDropDownOpen = !combo.IsDropDownOpen;
                e.Handled = true;
            }
        };

        //// ListView for dropdown items
        //ListView = new ListView()
        //{
        //    ItemTemplateType = typeof(ListViewItemTemplate),
        //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
        //    VerticalAlignment = VerticalAlignmentType.Stretch,
        //    Height = combo.DropDownHeight,
        //};

        // Header content: a Panel (not a StackPanel - StackPanel can only stretch its *last*
        // child to fill remaining space, not pin a fixed-size element to one edge while
        // another fills the rest) holding both EditBox and DisplayLabel (only one Visible at a
        // time, toggled in Arrange) plus the DropDownButton pinned to the right via its own
        // HorizontalAlignment.
        HeaderPanel = new Panel()
        {
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Stretch
        };
        HeaderPanel.AddChild(DisplayLabel);
        HeaderPanel.AddChild(EditBox);
        HeaderPanel.AddChild(DropDownButton);

        HeaderBorder.Content = HeaderPanel;

        // Root content is a StackPanel-like setup: header then list
        var container = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Top,
            //Height = 120, // Temporary fixed height for testing
        };

        container.AddChild(HeaderBorder);
        //container.AddChild(ListView);

        RootBorder.Content = container;
        Content = RootBorder;
        Content.Padding = new Thickness(4);
    }

    public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
    {
        var combo = ParentAs<ComboBox>();

        // Set the ListView data context to the filtered items (or full list if not editable)
        if (ListView != null)
        {
            var items = combo.GetFilteredItems();
            ListView.DataContext = items.Cast<object>().ToList();
        }

        // Control visibility of the list based on IsDropDownOpen
        if (ListView != null)
        {
            ListView.Visible.Value = combo.IsDropDownOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        //if (ListView.Visible.Value == Visibility.Visible)
        //{
        //    // Limit available height for the list when open (e.g. max 200px)
        //    //var maxListHeight = 200;
        //    //if (availableSize.Height > maxListHeight)
        //    //{
        //    //    availableSize.Height = maxListHeight;
        //    //}
        //}

        base.Measure(context, ref availableSize, ref parentMinMax);

        // nothing special - children will measure themselves
    }

    public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
    {
        var combo = ParentAs<ComboBox>();

        // Refresh the dropdown popup's filtered items here (layout phase) rather than only in
        // RenderControl below. All windows' layout runs before any window's render each frame
        // (see UIManager.Update/Draw), so a Text update only reflected in the popup's own
        // ListView.DataContext at render time is one full frame too late for that popup's own
        // Arrange (which builds/prunes its item list) to pick up - refreshing here as well lets
        // it see this frame's filtered results immediately instead of lagging by a frame.
        combo.RefreshPopupItems();

        // Show whichever of EditBox/DisplayLabel matches the current mode. Both are permanent
        // children of HeaderPanel (see InitTemplate) - toggling Visible instead of swapping
        // HeaderBorder.Content lets DropDownButton stay a stable sibling instead of being
        // re-added every pass, and naturally makes a hidden EditBox unfocusable (the mouse-down
        // focus search in UIManager.Mouse.cs requires Visible == Visible).
        if (EditBox != null)
        {
            // Assigning through .Value (mutating the existing bindings) rather than the
            // properties directly - the latter relies on Binding<T>'s implicit T->Binding<T>
            // conversion, which allocates a brand new Binding<T> (plus two delegates) every
            // single Arrange call, every frame, for every ComboBox.
            EditBox.Visible.Value = combo.IsEditable.Value ? Visibility.Visible : Visibility.Collapsed;

            // TextEntryControl hardcodes IsTabStop = true in its constructor, and Tab navigation
            // (UIWindow.HandleTabNext/HandleTabPrevious) selects the next stop by IsTabStop
            // alone - it doesn't check Visible. Left alone, that lets Tab land keyboard focus
            // on this EditBox even while it's Collapsed in non-editable mode, and once focused
            // the sync below pushes its typed text into combo.Text (and from there into the
            // visible DisplayLabel) regardless of IsEditable. Keeping IsTabStop in lockstep with
            // Visible closes that path.
            EditBox.IsTabStop.Value = combo.IsEditable.Value;

            // The real leak: UIComponent.HandleFocusChangedEventAsync (the base implementation)
            // propagates a "focused" event into *every* descendant of whatever component
            // actually receives focus, and each descendant sets its own HasFocus = true as part
            // of that - unconditionally, regardless of Visible or IsTabStop. So simply clicking
            // anywhere on the ComboBox header (landing focus on the ComboBox/HeaderPanel, not
            // EditBox specifically) still cascades HasFocus = true down onto this EditBox. From
            // there, TextEntryControl.HandleKeyAsync has no idea ComboBox.IsEditable exists - it only
            // checks its own HasFocus before consuming a keystroke. Force it back off every
            // frame here so a stray cascaded focus grant can never leave EditBox able to consume
            // typed input while non-editable (this runs before HandleKeyPressAsync gets to the
            // next keystroke, since Arrange runs every frame).
            if (!combo.IsEditable.Value && EditBox.HasFocus.Value)
            {
                EditBox.HasFocus.Value = false;
            }
        }
        if (DisplayLabel != null)
        {
            DisplayLabel.Visible.Value = combo.IsEditable.Value ? Visibility.Collapsed : Visibility.Visible;
        }

        // Arrange using base
        base.Arrange(context, layoutBounds, parentLayoutBounds);

        // Keep EditBox.Text and combo.Text in sync. This isn't a live two-way binding (Text's
        // setter copies the assigned value in rather than adopting the binding by reference -
        // same as every other SetField-backed property), so syncing unconditionally in one
        // direction every frame - as this used to do, always combo -> EditBox - stomped
        // every keystroke back to whatever combo.Text was before the user started typing.
        // Instead, follow whichever side is the current source of truth: while the box has
        // focus the user is editing it directly, so push its value up; otherwise combo.Text
        // is authoritative (e.g. set programmatically via SelectedItem, or reverted on
        // Enter/Escape/focus-loss), so push it down.
        if (EditBox != null)
        {
            // Guard against IsEditable == false here too, not just via Visible/IsTabStop above -
            // this is the actual point where a focused EditBox's typed text would otherwise leak
            // into combo.Text (and from there the visible DisplayLabel) regardless of mode.
            bool isEditBoxFocused = EditBox.HasFocus.Value && combo.IsEditable.Value;

            if (isEditBoxFocused)
            {
                // Auto-open the dropdown as soon as focus is gained, so the filtered items are
                // visible immediately rather than only once the user starts typing
                // (ComboBoxOpenTrigger.AutoOpenOnType, the default). Gated on the focus-gained
                // edge (see wasEditBoxFocused) rather than "focused and currently closed" so it
                // doesn't immediately re-open right after the user closes it with Escape while
                // still typing.
                if (!wasEditBoxFocused && !combo.IsDropDownOpen && combo.OpenTrigger.Value == ComboBoxOpenTrigger.AutoOpenOnType)
                {
                    combo.IsDropDownOpen = true;
                }

                combo.Text.Value = EditBox.Text.Value;
            }
            else
            {
                // Focus-lost edge: notify the combo box so it can enforce strict mode and close
                // the popup, mirroring HandleFocusChangedEventAsync's non-editable-mode
                // behavior - see ComboBox.HandleFocusLost for why this can't just be that
                // override firing normally in editable mode.
                if (wasEditBoxFocused)
                {
                    combo.HandleFocusLost();
                }

                EditBox.Text = combo.Text;
            }

            wasEditBoxFocused = isEditBoxFocused;
        }

        // Update label text when not editable or to reflect selected item
        if (DisplayLabel != null)
        {
            DisplayLabel.Text = combo.Text;
        }

        // Re-evaluate the header's hover/focus styling every frame. EditBox's focus (which
        // drives the "focused" look in editable mode - see HandleStateChange) changes on a
        // separate component from this template, with no event that notifies this template
        // when it does, so there's nothing else that would otherwise pick up the change.
        HandleStateChange();
    }

    public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
    {
        // Keep display text in sync before rendering
        var combo = ParentAs<ComboBox>();

        if (EditBox != null)
        {
            EditBox.Text = combo.Text;
        }

        if (DisplayLabel != null)
        {
            DisplayLabel.Text = combo.Text;
        }

        if (ListView != null)
        {
            ListView.DataContext = combo.GetFilteredItems().Cast<object>().ToList();
            ListView.Visible.Value = combo.IsDropDownOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        base.RenderControl(context, layoutBounds, parentTransform);
        //base.RenderControl(context, FinalContentRect, parentTransform);
        //base.RenderControl(context, ParentWindow.FinalContentRect, parentTransform);
    }

    protected override void HandleStateChange()
    {
        var combo = ParentAs<ComboBox>();

        // Normal State
        ApplyThemedValue(combo, RootBorder.BorderColor, nameof(ComboBox.BorderColor), Theme.Outline);
        ApplyThemedValue(combo, DisplayLabel.TextColor, nameof(ComboBox.TextColor), Theme.OnSurface);
        ApplyThemedValue(combo, RootBorder.Background, nameof(ComboBox.Background), Color.Transparent);

        // Hover State
        if (MouseHover.Value)
        {
            ApplyThemedValue(combo, RootBorder.BorderColor, nameof(ComboBox.BorderColor), Theme.Primary);
        }

        // Focused State - in editable mode the embedded EditBox holds keyboard focus directly
        // rather than the ComboBox itself (see ComboBox.IsEditBoxFocused), so check that too;
        // otherwise the header never shows as focused while the user is typing in it.
        bool isFocused = HasFocus.Value || (combo.IsEditable.Value && EditBox != null && EditBox.HasFocus.Value);
        if (isFocused)
        {
            ApplyThemedValue(combo, RootBorder.BorderColor, nameof(ComboBox.BorderColor), Theme.Primary);
            ApplyThemedValue(combo, RootBorder.Background, nameof(ComboBox.Background), Theme.SurfaceVariant);
        }
    }
}