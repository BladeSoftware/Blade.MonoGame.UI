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

        // border1 has render caching enabled (Border's own constructor turns it on
        // unconditionally) and label1's glyphs are part of what gets baked into that cache.
        // label1.Text is a getter/setter relay onto textBox.Text (see its own comment below),
        // not the same Binding<string> instance - so every keystroke mutates textBox.Text.Value
        // directly and never fires label1.Text's own Changed event, which is what the
        // cache-invalidation bubbling in UIComponent.BubbleInvalidation relies on. Without this,
        // border1's cached bitmap only refreshes when some unrelated Binding it owns directly
        // (e.g. BorderColor, on a focus/hover change) happens to change value too - typed
        // characters never appear until then. Tracked and compared fresh every frame (same
        // reasoning as indicatorColor below) rather than via the Changed event, so it stays
        // correct regardless of whether textBox.Text is ever wholesale-reassigned to a new
        // Binding<string> instance (e.g. adopting a view-model binding).
        private string lastCachedText;

        public TextBoxTemplate()
        {
            IsHitTestVisible = false;  // The template itself should not be hit-testable
            CanFocus = false;           // The template itself should not be focusable

            // EnsureBindingsWired (UIComponent.cs) only auto-subscribes to a control's PUBLIC
            // IBinding-typed properties (via reflection) - labelFontSize/labelColor/labelPosition/
            // labelPunchAlpha above are private fields, invisible to that scan, so
            // PropertyAnimationManager.Update()'s per-frame Apply() (which sets each one's .Value
            // to progressively interpolate them) never bubbled a cache invalidation on its own.
            // That went unnoticed while the caret was still frozen (see the Tenth-bug fix) since
            // TextBoxTemplate.RenderControl basically never ran anyway; once the caret/blink fix
            // made it run periodically again, the animation's actual per-frame progress became
            // invisible to any cached ancestor Border except on whatever sparse frame something
            // else (e.g. HasFocus, a real public Binding) happened to also invalidate - the label
            // could only ever be observed at its start or (well after the 80ms duration had
            // already elapsed internally) its end, never any frame in between - "jumping" rather
            // than easing. Subscribing directly here bypasses the public-property-only scan
            // entirely for these four internal-only, never-externally-bound animation targets.
            labelFontSize.Changed += BubbleInvalidation;
            labelColor.Changed += BubbleInvalidation;
            labelPosition.Changed += BubbleInvalidation;
            labelPunchAlpha.Changed += BubbleInvalidation;
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
            // Assigning through .Value (mutating the existing Border/Label bindings already
            // wired up by EnsureBindingsWired) rather than the properties directly - the latter
            // relies on Binding<T>'s implicit T->Binding<T> conversion, which allocates a brand
            // new Binding<T> (plus two delegates) on every single Measure call for every visible
            // TextBox, every frame, regardless of Variant/hasLabel/hasHelperText actually
            // changing.
            if (textBox.Variant == Variant.Standard)
            {
                border1.Background.Value = Color.Transparent;
                border1.CornerRadius.Value = new CornerRadius(0);
                border1.Margin.Value = new Thickness(0, hasLabel ? 9 : 0, 0, helperTextHeight);
                // With no label, match DisplayLabel/label1's own implicit zero vertical
                // padding exactly (rather than merely shrinking it) - a fixed-height host like
                // ComboBox's 40px header budgets for a plain Label's zero-padding footprint,
                // and even the "compact" 5px top/bottom this used to fall back to was still
                // enough to push the box past that budget and get clipped.
                border1.Padding.Value = hasLabel ? new Thickness(10, 11, 0, 5) : new Thickness(10, 0, 0, 0);

            }
            else if (textBox.Variant == Variant.Filled)
            {
                border1.Background.Value = textBox.IsEnabled.Value ? Theme.SurfaceVariant : Theme.OnDisabled;
                border1.CornerRadius.Value = new CornerRadius(8, 8, 0, 0);
                //border1.Margin.Value = new Thickness(0, hasLabel ? 9 : 0, 0, helperTextHeight);
                border1.Margin.Value = new Thickness(0, hasLabel ? 0 : 0, 0, helperTextHeight);
                border1.Padding.Value = hasLabel ? new Thickness(10, 11, 0, 5) : new Thickness(10, 0, 0, 0);
            }
            else if (textBox.Variant == Variant.Outlined)
            {
                border1.Background.Value = Color.Transparent;
                border1.CornerRadius.Value = new CornerRadius(8);
                border1.Margin.Value = new Thickness(0, hasLabel ? 5 : 0, 0, helperTextHeight);
                border1.Padding.Value = hasLabel ? new Thickness(10, 14, 10, 5) : new Thickness(10, 0, 10, 0);
            }

            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            // The blink timer must be driven from here, not RenderControl: RenderControl is
            // skipped entirely whenever an ancestor Border's render cache stays valid (see
            // UIComponentDrawable.RenderChildOrFromCache) - e.g. every TextBox shown inside the
            // Examples project's Section wrapper, whose inner Border caches by default - but
            // Arrange always runs every frame regardless of caching (it's a layout, not a draw,
            // pass). Only ticks (and only then bubbles a redraw) while this box actually has
            // focus, so an unfocused/inactive TextBox's ancestor cache is left alone.
            var textBox = ParentAs<TextBox>();
            if (textBox != null && textBox.HasFocus.Value)
            {
                double elapsedMillis = (DateTime.Now - cursorTime).TotalMilliseconds;
                if (elapsedMillis > 500)
                {
                    cursorTime = DateTime.Now;
                    cursorFlashOn = !cursorFlashOn;
                    BubbleInvalidation();
                }
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            // Must run before base.RenderControl below - that's what actually blits border1
            // from its cache (see the field comment on lastCachedText for why the cache can
            // otherwise go stale on every keystroke).
            string currentText = ParentAs<TextBox>()?.Text?.Value;
            if (currentText != lastCachedText)
            {
                lastCachedText = currentText;
                border1.InvalidateCache();
            }

            //border1.CornerRadius = new CornerRadius(0, 0, 0, 0);
            base.RenderControl(context, layoutBounds, parentTransform);

            // cursorFlashOn is now flipped in Arrange (see its own comment) - RenderControl just
            // reads whatever state it's already in.
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
                border1.BorderThickness.Value = new Thickness(isFocused ? 2 : 1);
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
                border1.BorderThickness.Value = (Thickness)0f;
                //border1.CornerRadius.Value = (CornerRadius)0f;
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

                    context.Renderer.DrawString(spriteBatch, helperTextBounds, textBox.HelperText, helperTextSizeFont, helperTextSizeColor, HorizontalAlignmentType.Left, VerticalAlignmentType.Top, layoutBounds);
                }

                // Display the Selection highlight and the Cursor
                string editText = textBox.Text.Value ?? "";
                SpriteFontBase font = FontService.GetFontOrDefault(textBox.FontName?.Value, textBox.FontSize?.Value);

                TextEntryVisuals.DrawSelectionAndCursor(
                    context, spriteBatch, editText, ComputeLiveTextRect(font, editText), layoutBounds,
                    textBox.SelectionStart, textBox.SelectionLength, textBox.CursorPosition,
                    cursorFlashOn, isFocused, font,
                    new Color(Theme.Primary, 0.35f), Theme.OnSurface);

                // Unconditionally re-widen the clip to layoutBounds as the LAST thing in this
                // batch, regardless of what ran above. UIRenderer.BeginBatch uses
                // SpriteSortMode.Deferred, which applies whatever scissor rect is live when the
                // batch actually flushes (at EndBatch) to EVERY draw already queued in the batch -
                // not whatever was live when each individual draw call was made. UIRenderer.
                // DrawString itself narrows the clip to Rectangle.Intersect(clippingRect, rectangle)
                // for whatever string it's drawing (see its own code) - so the floating label's
                // and helper text's own DrawString calls above each leave the clip narrowed to
                // THEIR OWN bounds afterward. Passing layoutBounds into DrawSelectionAndCursor
                // (above) re-widens it, but only when that FillRect actually executes
                // (cursorFlashOn && isFocused, or an active selection) - on any frame where
                // neither fires, the LAST clip left active was the helper text's own narrow
                // Intersect(layoutBounds, helperTextBounds), which - because Deferred applies it
                // to the whole batch, not just the helper text draw - retroactively clipped away
                // the underline and floating label (both positioned above the helper text's row)
                // too. This is what caused "the underline and label blink" as a side effect of
                // fixing the caret to actually blink: half the blink cycle now correctly
                // re-widens the clip (whenever the caret draws), the other half doesn't.
                context.Renderer.ClipToRect(layoutBounds);
            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

        /// <summary>
        /// Recomputes where label1's text is currently drawn (its Left/Bottom-aligned single
        /// line, per TextEntryControl's constructor), without depending on label1.TextRect -
        /// which is only ever *set* inside LabelTemplate.RenderControl, itself only reachable
        /// through border1's own RenderChildOrFromCache check (border1 - a Border - always
        /// enables caching). Whenever border1's cached texture stays valid, that whole render
        /// path (and therefore the TextRect write) is skipped, leaving label1.TextRect frozen at
        /// wherever it was last actually drawn - which used to be a non-issue back when
        /// TextBoxTemplate.RenderControl itself rarely ran without border1 also refreshing at
        /// the same time (e.g. a focus/hover color change invalidates border1 directly), but
        /// became visible once the caret/blink fix started forcing TextBoxTemplate.RenderControl
        /// to run on its own timer/on cursor moves - reusing a stale TextRect from an earlier,
        /// unrelated layout state (e.g. before the floating label animation settled), which could
        /// land on top of the helper text drawn below. label1.FinalContentRect, unlike TextRect,
        /// is kept current by Arrange every single frame regardless of Draw-time caching, so
        /// recomputing from that is never stale.
        /// </summary>
        private Rectangle ComputeLiveTextRect(SpriteFontBase font, string text)
        {
            Vector2 lineSize = font.MeasureString(string.IsNullOrEmpty(text) ? " " : text);
            Rectangle contentRect = label1.FinalContentRect;

            return new Rectangle(contentRect.Left, contentRect.Bottom - (int)lineSize.Y, (int)lineSize.X, (int)lineSize.Y);
        }

        /// <summary>
        /// Maps an absolute screen X coordinate to the nearest character index in the text
        /// box's current text, based on where the text is currently drawn (see
        /// ComputeLiveTextRect). Used by TextBox for click-to-position-caret and click-drag
        /// selection.
        /// </summary>
        public int GetCharacterIndexAtX(float screenX)
        {
            var textBox = ParentAs<TextBox>();
            string text = textBox.Text.Value ?? "";
            SpriteFontBase font = FontService.GetFontOrDefault(textBox.FontName?.Value, textBox.FontSize?.Value);

            return TextEntryVisuals.GetCharacterIndexAtX(screenX, text, ComputeLiveTextRect(font, text), font);
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
