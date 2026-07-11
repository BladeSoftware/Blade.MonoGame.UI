using Blade.MG.UI.Animations;
using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Blade.MG.UI.Services;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class TextBoxTemplate : Control, ITextEntryTemplate
    {
        private Border border1;
        private Label label1;

        private DateTime cursorTime;
        private bool cursorFlashOn = false;

        // Eases the floating Label between its "expanded" (centered, large) and "shrunk"
        // (top-left, small) appearance instead of jumping - see PropertyAnimationManager in
        // Blade.MG.UI.Animations. Each Binding below tracks one real rendered property
        // directly (no synthetic 0..1 blend factor to manually Lerp everything else from).
        private readonly Binding<float> labelFontSize = new Binding<float>(0f);
        private readonly Binding<Color> labelColor = new Binding<Color>(Color.Transparent);
        private readonly Binding<Vector2> labelPosition = new Binding<Vector2>(Vector2.Zero);

        // Outlined punches a hole behind the label to erase the border stroke passing behind
        // it (see the FillRect call below) - its alpha is a real rendered property in its own
        // right, animated the same way as the three above.
        private readonly Binding<float> labelPunchAlpha = new Binding<float>(0f);

        // Guards the very first RenderControl call, snapping straight to whatever state the
        // box should already be in (e.g. pre-filled Text) rather than animating in from the
        // meaningless zeroed-out defaults above.
        private bool labelAnimationInitialized = false;

        public TextBoxTemplate()
        {
            IsHitTestVisible = false;  // The template itself should not be hit-testable
            CanFocus = false;           // The template itself should not be focusable
        }

        protected override void InitTemplate()
        {
            var textBox = ParentAs<TextBox>();

            // Link to the TextBox's own Background binding so a developer can override it
            // directly (textBox.Background = ...) as well as via SetStyleOverride.
            Background = textBox.Background;
            ApplyThemedValue(textBox, Background, nameof(TextBox.Background), Theme.Surface);

            //Margin = new Thickness(0, 0, 0, 7); // Reserve space for the helper text
            //Padding = new Thickness(0, 7, 0, 0);

            border1 = new Border
            {
                // Link to the TextBox's own BorderColor binding so a developer can override it
                // directly (textBox.BorderColor = ...) as well as via SetStyleOverride.
                BorderColor = textBox.BorderColor,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(0),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,

                // Prevent child controls from receiving input events
                IsHitTestVisible = false,
                CanFocus = false
            };

            label1 = new Label()
            {
                // Linked via a getter/setter Binding rather than a plain `Text = textBox.Text`
                // snapshot assignment. Label.Text is a plain auto-property, so that bare
                // assignment *looks* like it adopts textBox's own Binding<string> by
                // reference (making every future read/write of either side visible through
                // both) - but in practice label1's text never reflected what was typed or
                // programmatically selected. Building the Binding from a getter/setter pair
                // instead means every access reads/writes straight through to textBox.Text.Value
                // itself, live, with nothing to snapshot or fall out of sync - so it holds up
                // regardless of what was breaking the reference-sharing assumption above.
                Text = new Binding<string>(() => textBox.Text.Value, v => textBox.Text.Value = v),
                Background = Color.Transparent,
                TextColor = new Binding<Color>(() => textBox.TextColor.Value, v => textBox.TextColor.Value = v),
                HorizontalTextAlignment = new Binding<HorizontalAlignmentType>(() => textBox.HorizontalTextAlignment.Value, v => textBox.HorizontalTextAlignment.Value = v),
                VerticalTextAlignment = new Binding<VerticalAlignmentType>(() => textBox.VerticalTextAlignment.Value, v => textBox.VerticalTextAlignment.Value = v),

                // FontName/FontSize are left as plain snapshot assignments (not linked): unlike
                // the properties above, TextBox allows these to be a literal null Binding
                // (meaning "use the default font"), and LabelTemplate already reads them via
                // null-conditional access (label.FontName?.Value) - a live getter closure can't
                // return null through a non-nullable Binding<T> the same way, and neither
                // FontName nor FontSize changes after a TextBox is constructed in any current
                // usage, so there's no staleness risk here to link against.
                FontName = textBox.FontName,
                FontSize = textBox.FontSize,

                // Prevent child controls from receiving input events
                IsHitTestVisible = false,
                CanFocus = false
            };


            border1.Content = label1;

            Content = border1;

            cursorTime = DateTime.Now;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            var textBox = ParentAs<TextBox>();

            // These margins/paddings exist purely to reserve room for the floating label
            // (above/inside the box) and the helper text (below it) - both drawn by
            // RenderControl below. A TextBox with neither set (e.g. ComboBox's embedded
            // editable header, which uses TextBox purely as a compact input with no Material
            // label/helper chrome) has nothing to draw there, so reserving the space anyway
            // just inflates the control well past what its content needs - which is exactly
            // what was blowing out ComboBox's fixed-height header.
            bool hasLabel = !string.IsNullOrEmpty(textBox.Label);
            bool hasHelperText = !string.IsNullOrEmpty(textBox.HelperText);

            int helperTextHeight = hasHelperText ? 16 : 0;

            // Border color/thickness are state-driven (hover/focus - see HandleStateChange),
            // not just Variant-driven, so they're applied there instead of here - Measure()
            // still owns everything else Variant controls (fill, corner radius, spacing).
            if (textBox.Variant == Variant.Standard)
            {
                border1.Background = Color.Transparent;
                border1.CornerRadius = new CornerRadius(0);
                border1.Margin = new Thickness(0, hasLabel ? 9 : 0, 0, helperTextHeight);
                // With no label, match DisplayLabel/label1's own implicit zero vertical
                // padding exactly (rather than merely shrinking it) - a fixed-height host like
                // ComboBox's 40px header budgets for a plain Label's zero-padding footprint,
                // and even the "compact" 5px top/bottom this used to fall back to was still
                // enough to push the box past that budget and get clipped.
                border1.Padding = hasLabel ? new Thickness(10, 11, 0, 5) : new Thickness(10, 0, 0, 0);

            }
            else if (textBox.Variant == Variant.Filled)
            {
                border1.Background = textBox.IsEnabled.Value ? Theme.SurfaceVariant : Theme.OnDisabled;
                border1.CornerRadius = new CornerRadius(8, 8, 0, 0);
                //border1.Margin = new Thickness(0, hasLabel ? 9 : 0, 0, helperTextHeight);
                border1.Margin = new Thickness(0, hasLabel ? 0 : 0, 0, helperTextHeight);
                border1.Padding = hasLabel ? new Thickness(10, 11, 0, 5) : new Thickness(10, 0, 0, 0);
            }
            else if (textBox.Variant == Variant.Outlined)
            {
                border1.Background = Color.Transparent;
                border1.CornerRadius = new CornerRadius(8);
                border1.Margin = new Thickness(0, hasLabel ? 5 : 0, 0, helperTextHeight);
                border1.Padding = hasLabel ? new Thickness(10, 14, 10, 5) : new Thickness(10, 0, 10, 0);
            }

            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            //border1.CornerRadius = new CornerRadius(0, 0, 0, 0);
            base.RenderControl(context, layoutBounds, parentTransform);

            double elapsedMillis = (DateTime.Now - cursorTime).TotalMilliseconds;
            if (elapsedMillis > 500)
            {
                cursorTime = DateTime.Now;
                cursorFlashOn = !cursorFlashOn;
            }

            var textBox = ParentAs<TextBox>();

            // Recomputed fresh every frame - rather than only reactively in HandleStateChange -
            // so it can never go stale regardless of event-cascade ordering/timing (this control
            // is instantiated twice per TextBox - see TemplatedControl.InitTemplate's own
            // internal-child copy alongside this one, assigned as Content - and relying on
            // HandleFocusChangedEventAsync alone left a window where a later, stale
            // HandleHoverChangedAsync-triggered HandleStateChange call could overwrite the
            // correct focused color with the hover color, only correcting itself once the mouse
            // actually left and re-triggered HandleStateChange one more time).
            // isFocused/isHovered fold in IsEnabled so a disabled field (which can still
            // incidentally hold focus/hover - see TextEntryControl's own IsEnabled guards, which
            // only block *editing*, not hit-testing) never shows the interactive-looking hover/
            // focus treatment, only the dimmed Disabled color below.
            bool isEnabled = textBox.IsEnabled.Value;
            bool isFocused = isEnabled && textBox.HasFocus.Value;
            bool isHovered = isEnabled && MouseHover.Value;
            Color indicatorColor = !isEnabled ? Theme.Disabled : (isFocused ? Theme.Primary : (isHovered ? Theme.OnSurface : Theme.OnSurfaceVariant));

            ApplyThemedValue(textBox, textBox.UnderlineColor, nameof(TextEntryControl.UnderlineColor), indicatorColor);
            ApplyThemedValue(textBox, textBox.TextColor, nameof(TextBox.TextColor), isEnabled ? Theme.OnSurface : Theme.Disabled);

            if (textBox.Variant == Variant.Outlined)
            {
                ApplyThemedValue(textBox, border1.BorderColor, nameof(TextBox.BorderColor), indicatorColor);
                border1.BorderThickness = new Thickness(isFocused ? 2 : 1);
            }
            else
            {
                ApplyThemedValue(textBox, border1.BorderColor, nameof(TextBox.BorderColor), Color.Transparent);

                // Zero, not 1 - Border.RenderControl's rounded-corner path insets its own
                // Background fill by BorderThickness for the stencil mask *regardless* of
                // whether a border stroke actually gets drawn (that only happens when
                // BorderColor isn't transparent, per Border's own hasBorder check). A nonzero
                // thickness here with rounded corners (Filled has CornerRadius(8,8,0,0)) shrunk
                // the fill by 1px on every edge for a border that was never visible anyway,
                // leaving a 1px seam of this template's own background peeking through right at
                // the top, exactly where filledRect's patch meets border1's own fill.
                border1.BorderThickness = (Thickness)0f;
                //border1.CornerRadius = (CornerRadius)0f;
            }

            try
            {
                var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(layoutBounds);

                // If the Variant = Filled, then fill the textbox including the top margin.
                // Anchored to textBox.FinalRect.Top (the same point the shrunk floating label
                // is positioned from, below) rather than derived from border1.Margin.Value.Top -
                // the two didn't always land on the same pixel, leaving a thin sliver of this
                // template's own Theme.Surface background (near-white) visible above the fill,
                // right where the floating label sits, looking like a stray white top border.
                if (textBox.Variant == Variant.Filled)
                {
                    Rectangle filledRect = border1.FinalRect with { Y = textBox.FinalRect.Top, Height = border1.FinalRect.Top - textBox.FinalRect.Top };
                    //context.Renderer.FillRect(spriteBatch, filledRect, border1.Background.Value);

                    // Material 3 hover state layer: a subtle overlay tint shown only while
                    // hovered - not focused (focus already has its own distinct treatment) and
                    // not disabled (isHovered is already false whenever !isEnabled).
                    if (isHovered && !isFocused)
                    {
                        context.Renderer.FillRect(spriteBatch, filledRect, new Color(Theme.OnSurface, 0.08f));
                        context.Renderer.FillRect(spriteBatch, border1.FinalRect, new Color(Theme.OnSurface, 0.08f));
                    }
                }

                // Display the underline (active indicator) - thickens on focus, matching
                // Material 3's filled/standard text field indicator behavior.
                if (textBox.Underline)
                {
                    int thickness = isFocused ? 2 : 1;
                    Rectangle underlineRect;

                    if (textBox.Variant == Variant.Standard)
                    {
                        underlineRect = new Rectangle(label1.FinalRect.X, border1.FinalRect.Bottom - thickness, border1.FinalRect.Width, thickness);
                        context.Renderer.FillRect(spriteBatch, underlineRect, textBox.UnderlineColor.Value);
                    }
                    else if (textBox.Variant == Variant.Filled)
                    {
                        underlineRect = new Rectangle(border1.FinalRect.X, border1.FinalRect.Bottom - thickness, border1.FinalRect.Width, thickness);
                        context.Renderer.FillRect(spriteBatch, underlineRect, textBox.UnderlineColor.Value);
                    }

                }


                // Display the Label - eases its font size/color/position/punch-hole alpha
                // individually via PropertyAnimationManager instead of jumping instantly.

                // If the text box has focus OR contains text OR ShrinkLabel is forced on, then shrink the label
                bool shrinkLabel = isFocused || textBox.ShrinkLabel;
                if (!string.IsNullOrEmpty(textBox.Text?.Value))
                {
                    shrinkLabel = true;
                }

                float targetFontSize;
                Color targetColor;
                Vector2 targetPosition;
                float targetPunchAlpha;

                if (shrinkLabel)
                {
                    // Shrunk: top-left corner, small fixed size - only actually focused gets
                    // the Primary "active" color (Material 3); has-text-but-unfocused looks the
                    // same as the unfocused/empty state, just in the shrunk position.
                    targetFontSize = 15f;
                    targetColor = !isEnabled ? Theme.Disabled : (isFocused ? Theme.Primary : Theme.OnSurfaceVariant);
                    float shrunkTopY = textBox.Variant == Variant.Outlined ? border1.FinalRect.Top - 7 : textBox.FinalRect.Top + 2;
                    float shrunkX = FinalContentRect.Left + 5 + 5;
                    targetPosition = new Vector2(shrunkX, shrunkTopY);
                    targetPunchAlpha = 1f;
                }
                else
                {
                    // Expanded: centered in the box, large, OnSurfaceVariant/Disabled.
                    targetFontSize = textBox.FontSize?.Value ?? FontService.DefaultFontSize;
                    targetColor = isEnabled ? Theme.OnSurfaceVariant : Theme.Disabled;

                    SpriteFontBase expandedFont = FontService.GetFontOrDefault(textBox.FontName?.Value, targetFontSize);
                    Rectangle expandedLabelBounds = new Rectangle(FinalContentRect.X, label1.FinalContentRect.Y, FinalContentRect.Width, label1.FinalContentRect.Height);
                    expandedLabelBounds = expandedLabelBounds with { X = expandedLabelBounds.X + 8 };
                    // Replicate UIRenderer.DrawString's own VerticalAlignmentType.Center math
                    // (centers on cap-height, not full glyph height, clamped to the rect's own
                    // top) - needed as a concrete Y position since Center/Top can't be
                    // cross-faded as enum values.
                    float expandedTopY = MathF.Max(expandedLabelBounds.Top, (expandedLabelBounds.Top + expandedLabelBounds.Bottom) / 2f - expandedFont.MeasureString("I").Y / 2f);

                    targetPosition = new Vector2(expandedLabelBounds.X, expandedTopY);
                    targetPunchAlpha = 0f;
                }

                if (!labelAnimationInitialized)
                {
                    // Snap straight to whatever state the box should already be in (e.g.
                    // pre-filled Text) rather than animating in from a meaningless zeroed-out
                    // default on the very first paint.
                    labelFontSize.Value = targetFontSize;
                    labelColor.Value = targetColor;
                    labelPosition.Value = targetPosition;
                    labelPunchAlpha.Value = targetPunchAlpha;
                    labelAnimationInitialized = true;
                }
                else
                {
                    // Idempotent - a no-op once already animating toward (or at rest at) the
                    // given target, so these can be called unconditionally every frame with no
                    // separate "did the target change" tracking.
                    PropertyAnimationManager.AnimateTo(labelFontSize, targetFontSize, TimeSpan.FromMilliseconds(80), Easing.EaseOutCubic);
                    PropertyAnimationManager.AnimateTo(labelColor, targetColor, TimeSpan.FromMilliseconds(80), Easing.EaseOutCubic);
                    PropertyAnimationManager.AnimateTo(labelPosition, targetPosition, TimeSpan.FromMilliseconds(80), Easing.EaseOutCubic);
                    PropertyAnimationManager.AnimateTo(labelPunchAlpha, targetPunchAlpha, TimeSpan.FromMilliseconds(80), Easing.EaseOutCubic);
                }

                SpriteFontBase lerpedFont = FontService.GetFontOrDefault(textBox.FontName?.Value, labelFontSize.Value);
                Vector2 lerpedTextSize = lerpedFont.MeasureString(textBox.Label);
                Rectangle lerpedLabelBounds = new Rectangle((int)labelPosition.Value.X + 5, (int)labelPosition.Value.Y, (int)lerpedTextSize.X + 10, (int)lerpedTextSize.Y);
                Rectangle lerpedLabelBackground = new Rectangle((int)labelPosition.Value.X, (int)labelPosition.Value.Y, (int)lerpedTextSize.X + 10, (int)lerpedTextSize.Y);

                // Outlined punches a hole behind the label to erase the border stroke passing
                // behind it - fade the hole in/out proportionally with the label's own motion
                // (a no-op at labelPunchAlpha == 0, since the label is nowhere near the border
                // there) rather than an abrupt on/off threshold.
                if (textBox.Variant == Variant.Outlined)
                {
                    context.Renderer.FillRect(spriteBatch, lerpedLabelBackground, new Color(textBox.Background.Value, labelPunchAlpha.Value));
                    //context.Renderer.FillRect(spriteBatch, lerpedLabelBounds, new Color(textBox.Background.Value, labelPunchAlpha.Value));
                }

                context.Renderer.DrawString(spriteBatch, lerpedLabelBounds, textBox.Label, lerpedFont, labelColor.Value, HorizontalAlignmentType.Left, VerticalAlignmentType.Top, layoutBounds);


                // Display the helper text
                if (!string.IsNullOrEmpty(textBox.HelperText))
                {

                    // Text Box has Text, so display Label at the top-left corner of the text box
                    Color helperTextSizeColor = isEnabled ? Theme.OnSurfaceVariant : Theme.Disabled;
                    float helperTextSizeFontSize = 14;

                    SpriteFontBase helperTextSizeFont = FontService.GetFontOrDefault(textBox.FontName?.Value, helperTextSizeFontSize);
                    Vector2 helperTextSize = helperTextSizeFont.MeasureString(textBox.HelperText);

                    Rectangle helperTextBounds;

                    helperTextBounds = new Rectangle(FinalContentRect.Left + 2, border1.FinalRect.Bottom + 3, (int)helperTextSize.X + 10, (int)helperTextSize.Y);

                    if (textBox.Variant == Variant.Standard)
                    {
                        helperTextBounds = helperTextBounds with { X = helperTextBounds.X + 8 };
                    }

                    context.Renderer.DrawString(spriteBatch, helperTextBounds, textBox.HelperText, helperTextSizeFont, helperTextSizeColor, HorizontalAlignmentType.Left, VerticalAlignmentType.Top, textBox.FinalRect);
                }

                // Display the Selection highlight and the Cursor
                string editText = textBox.Text.Value ?? "";
                SpriteFontBase font = FontService.GetFontOrDefault(textBox.FontName?.Value, textBox.FontSize?.Value);

                TextEntryVisuals.DrawSelectionAndCursor(
                    context, spriteBatch, editText, label1.TextRect, label1.FinalContentRect,
                    textBox.SelectionStart, textBox.SelectionLength, textBox.CursorPosition,
                    cursorFlashOn, isFocused, font,
                    new Color(Theme.Primary, 0.35f), Theme.OnSurface);

            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

        /// <summary>
        /// Maps an absolute screen X coordinate to the nearest character index in the text
        /// box's current text, based on the text's last-rendered position (label1.TextRect).
        /// Used by TextBox for click-to-position-caret and click-drag selection.
        /// </summary>
        public int GetCharacterIndexAtX(float screenX)
        {
            var textBox = ParentAs<TextBox>();
            string text = textBox.Text.Value ?? "";
            SpriteFontBase font = FontService.GetFontOrDefault(textBox.FontName?.Value, textBox.FontSize?.Value);

            return TextEntryVisuals.GetCharacterIndexAtX(screenX, text, label1.TextRect, font);
        }


        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            StateHasChanged();
        }


        //public T GetResourceViewState<T>(string property)
        //{
        //    string resourceKey = ResourceKey;
        //    return GetResourceValue<T>($"{property}{ViewState()}");
        //}


        protected override void HandleStateChange()
        {
            var textBox = ParentAs<TextBox>();

            ApplyThemedValue(textBox, Background, nameof(TextBox.Background), Theme.Surface);

            // Text/border/underline/label color and border thickness are all recomputed fresh
            // every frame in RenderControl instead of here - see the comment there for why.
        }

    }
}
