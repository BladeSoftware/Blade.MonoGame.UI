using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Threading;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Reproduces the bug where a focused TextBox's caret stopped blinking, and stopped moving
    /// when the arrow keys repositioned it, once the box was nested inside a cached ancestor
    /// Border (e.g. the Examples project's Section wrapper - Border enables render caching by
    /// default, see Border's own constructor). Root cause: TextBoxTemplate.RenderControl - which
    /// draws the caret directly via TextEntryVisuals.DrawSelectionAndCursor - is skipped entirely
    /// whenever the ancestor's cached texture stays valid (see
    /// UIComponentDrawable.RenderChildOrFromCache), and neither CursorPosition (a plain int, not
    /// a Binding&lt;T&gt;) nor the wall-clock-driven blink timer ever fired a cache invalidation,
    /// so nothing ever forced that ancestor's cache to refresh.
    /// </summary>
    [TestClass]
    public class TestTextBoxCaretCacheInvalidation
    {
        private static (FakeUIManager uiManager, Border border, TextBox textBox, RenderTarget2D renderTarget, SpriteBatch spriteBatch) BuildFocusedTextBoxInCachedBorder()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(host);

            // Mirrors Examples\GameStudio\UI\HelpPages\HelpPage_TextBox.cs's Section.BuildField
            // wrapper - a Border with EnableCaching (the same thing Section's own inner Border
            // uses, defaulted on) around the TextBox, not the TextBox's own internal border1.
            var border = new Border
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(0f),
                BorderThickness = new Thickness(0),
                Background = Color.Transparent,
                EnableCaching = true,
            };
            host.AddChild(border);

            var textBox = new TextBox
            {
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Text = "ABCDEFGHIJKLMNOP",
                Label = null,
                HelperText = null,
            };
            border.Content = textBox;

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            textBox.HasFocus.Value = true;

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            var spriteBatch = new SpriteBatch(graphicsDevice);

            return (uiManager, border, textBox, renderTarget, spriteBatch);
        }

        private static long SampleBorderChecksum(FakeUIManager uiManager, Border border, RenderTarget2D renderTarget, SpriteBatch spriteBatch)
        {
            var graphicsDevice = FakeGame.Instance.GraphicsDevice;

            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Black);

            uiManager.Draw(spriteBatch, new GameTime(), renderTarget);

            graphicsDevice.SetRenderTarget(null);

            Color[] pixels = new Color[renderTarget.Width * renderTarget.Height];
            renderTarget.GetData(pixels);

            Rectangle rect = border.GetFinalRect();
            long checksum = 0;

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    Color c = pixels[y * renderTarget.Width + x];
                    checksum += c.R + (c.G * 31) + (c.B * 97);
                }
            }

            return checksum;
        }

        [TestMethod]
        public void CursorPositionChange_WhileNestedInCachedBorder_MovesTheRenderedCaret()
        {
            var (uiManager, border, textBox, renderTarget, spriteBatch) = BuildFocusedTextBoxInCachedBorder();
            using (renderTarget)
            using (spriteBatch)
            {
                textBox.CursorPosition = 0;

                long stableChecksum = 0;
                for (int i = 0; i < 4; i++)
                {
                    stableChecksum = SampleBorderChecksum(uiManager, border, renderTarget, spriteBatch);
                }

                long repeatChecksum = SampleBorderChecksum(uiManager, border, renderTarget, spriteBatch);
                Assert.AreEqual(stableChecksum, repeatChecksum, "Expected stable pixels frame-over-frame with nothing changing in between.");

                textBox.CursorPosition = textBox.Text.Value.Length;

                long afterMoveChecksum = SampleBorderChecksum(uiManager, border, renderTarget, spriteBatch);

                Assert.AreNotEqual(stableChecksum, afterMoveChecksum, "Expected the rendered caret to move (changing the ancestor Border's cached pixels) after CursorPosition changed - equal checksums mean the cache went stale instead of being invalidated by the caret moving, matching the reported 'caret position doesn't refresh when moving it with arrow keys' bug.");
            }
        }

        [TestMethod]
        public void BlinkTimer_WhileFocused_ForcesTheAncestorCacheToRefreshOverTime()
        {
            var (uiManager, border, textBox, renderTarget, spriteBatch) = BuildFocusedTextBoxInCachedBorder();
            using (renderTarget)
            using (spriteBatch)
            {
                textBox.CursorPosition = 4;

                long firstChecksum = SampleBorderChecksum(uiManager, border, renderTarget, spriteBatch);

                // The blink interval is 500ms (TextBoxTemplate.Arrange) - run enough
                // PerformLayout passes spanning past that to give it a chance to flip and bubble
                // an invalidation. Real time, matching the existing hover-delay tests' approach
                // (e.g. TestTooltip.cs) rather than mocking the clock.
                bool sawChange = false;
                long lastChecksum = firstChecksum;
                for (int i = 0; i < 10 && !sawChange; i++)
                {
                    Thread.Sleep(120);
                    uiManager.PerformLayout();
                    long checksum = SampleBorderChecksum(uiManager, border, renderTarget, spriteBatch);
                    if (checksum != lastChecksum)
                    {
                        sawChange = true;
                    }
                    lastChecksum = checksum;
                }

                Assert.IsTrue(sawChange, "Expected the caret's blink to change the ancestor Border's cached pixels at least once within 1.2s - no change means the blink is frozen under the cached ancestor, matching the reported 'cursor no longer blinks' bug.");
            }
        }

        /// <summary>
        /// Reproduces a follow-up bug found after the two fixes above shipped: "the helper text
        /// disappears when the cursor blinks on, and the cursor shows at the wrong vertical
        /// position." Root cause: label1.TextRect (Label.cs) is only ever *written* inside
        /// LabelTemplate.RenderControl - itself only reachable when border1's own render cache
        /// is invalid (border1, a Border, always caches). Once the caret/blink fix above started
        /// forcing TextBoxTemplate.RenderControl to run on its own timer/on cursor moves -
        /// *without* necessarily touching any of border1's own bindings - label1.TextRect stayed
        /// frozen at whatever it was the last time border1 itself actually redrew (e.g. an
        /// earlier layout pass), while the caret/selection code kept reading it as if it were
        /// current. Fixed by TextBoxTemplate.ComputeLiveTextRect, which derives the text row
        /// fresh from label1.FinalContentRect (kept current by Arrange every frame, regardless
        /// of Draw-time caching) instead of the Draw-time-only TextRect field.
        /// </summary>
        [TestMethod]
        public void Caret_DoesNotOverwriteHelperText_EvenWhenLabel1TextRectIsStale()
        {
            var (uiManager, border, textBox, renderTarget, spriteBatch) = BuildFocusedTextBoxInCachedBorder();
            using (renderTarget)
            using (spriteBatch)
            {
                textBox.HelperText = "Helper text";
                textBox.CursorPosition = 4;

                // Warm up: let layout and border1's own cache settle normally, so label1.TextRect
                // is legitimately correct before we deliberately corrupt it below.
                for (int i = 0; i < 4; i++)
                {
                    SampleBorderChecksum(uiManager, border, renderTarget, spriteBatch);
                }

                object template = textBox.Content;
                FieldInfo label1Field = template.GetType().GetField("label1", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo cursorFlashField = template.GetType().GetField("cursorFlashOn", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo border1Field = template.GetType().GetField("border1", BindingFlags.NonPublic | BindingFlags.Instance);

                var label1 = (Label)label1Field.GetValue(template);
                var border1 = (Border)border1Field.GetValue(template);

                // Corrupt label1.TextRect to sit exactly where the helper text is drawn
                // (TextBoxTemplate.RenderControl positions it at border1.FinalRect.Bottom + 3) -
                // simulating the reported staleness landing squarely on top of the helper text.
                Rectangle corruptedTextRect = new Rectangle(label1.FinalContentRect.Left, border1.FinalRect.Bottom + 3, 60, 16);
                label1.TextRect = corruptedTextRect;

                // Force the caret into its visible "on" phase deterministically, rather than
                // waiting on real time (see BlinkTimer_WhileFocused above for the real-time version).
                cursorFlashField.SetValue(template, true);

                // Move the cursor - bubbles an invalidation forcing TextBoxTemplate.RenderControl
                // to run again, without touching any of border1's own bindings, so border1 itself
                // does not redraw and label1.TextRect is NOT refreshed - it stays corrupted.
                textBox.CursorPosition = 2;
                cursorFlashField.SetValue(template, true);

                SampleBorderChecksum(uiManager, border, renderTarget, spriteBatch);

                Color[] pixels = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(pixels);

                Color cursorColor = textBox.Theme.OnSurface;
                bool cursorColorFoundInHelperTextBand = false;

                for (int y = corruptedTextRect.Top; y < corruptedTextRect.Bottom && !cursorColorFoundInHelperTextBand; y++)
                {
                    for (int x = corruptedTextRect.Left; x < corruptedTextRect.Right; x++)
                    {
                        if (pixels[y * renderTarget.Width + x] == cursorColor)
                        {
                            cursorColorFoundInHelperTextBand = true;
                            break;
                        }
                    }
                }

                Assert.IsFalse(cursorColorFoundInHelperTextBand, "Expected the caret to never be drawn over the helper text's row, even with a corrupted/stale label1.TextRect - finding the caret's exact color there means the caret is still using the stale TextRect instead of a freshly-computed one, matching the reported 'helper text disappears when the cursor blinks on' bug.");
            }
        }

        /// <summary>
        /// Reproduces the actual root cause behind "the helper text disappears when the cursor
        /// blinks on, and shows again when it blinks off" - a SpriteBatch scissor-rect bleed, not
        /// a caching/staleness bug. UIRenderer.BeginBatch uses SpriteSortMode.Deferred (see its
        /// own comment), which applies whatever GraphicsDevice.ScissorRectangle is live when the
        /// batch actually flushes (at EndBatch) to *every* draw made in that Begin/End, not
        /// whatever was live when each individual draw call happened. TextBoxTemplate.RenderControl
        /// sets the scissor to layoutBounds once at the top of its batch (covering the floating
        /// label and helper text draws), but TextEntryVisuals.DrawSelectionAndCursor's caret/
        /// selection FillRect calls used to pass a narrower clippingRect (label1.FinalContentRect)
        /// - and that FillRect (and therefore its ClipToRect call) only runs when
        /// cursorFlashOn &amp;&amp; hasFocus is true. So on an "on" blink frame, the scissor rect
        /// live at flush time narrows to label1.FinalContentRect, retroactively clipping away the
        /// helper text (drawn earlier in the same batch, outside that narrower rect) too; on an
        /// "off" blink frame, that FillRect never runs, the scissor stays at the wider
        /// layoutBounds, and the helper text renders normally - exactly the reported blinking.
        /// </summary>
        [TestMethod]
        public void HelperText_StaysVisible_RegardlessOfCursorBlinkPhase()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(host);

            // Taller than BuildFocusedTextBoxInCachedBorder's shared 60px border, to give the
            // helper text (drawn below the 40px-tall TextBox itself) comfortable room to fit
            // inside the outer cached Border's bounds.
            var border = new Border
            {
                Width = 200,
                Height = 100,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(0f),
                BorderThickness = new Thickness(0),
                Background = Color.Transparent,
                EnableCaching = true,
            };
            host.AddChild(border);

            var textBox = new TextBox
            {
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Text = "ABCDEFGHIJKLMNOP",
                Label = null,
                HelperText = "Helper text",
            };
            border.Content = textBox;

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            textBox.HasFocus.Value = true;
            textBox.CursorPosition = 4;

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            object template = textBox.Content;
            FieldInfo cursorFlashField = template.GetType().GetField("cursorFlashOn", BindingFlags.NonPublic | BindingFlags.Instance);

            Color[] SamplePixelsWithBlinkPhase(bool cursorFlashOn)
            {
                cursorFlashField.SetValue(template, cursorFlashOn);
                // Directly mutating the private field via reflection bypasses BubbleInvalidation
                // entirely - the outer Border's own cache has no idea anything changed, so
                // without this it would just blit the SAME cached texture from the previous
                // sample every time. Force it invalid so each sample actually re-renders.
                border.InvalidateCache();

                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Black);

                uiManager.Draw(spriteBatch, new GameTime(), renderTarget);

                graphicsDevice.SetRenderTarget(null);

                Color[] pixels = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(pixels);
                return pixels;
            }

            // Warm up so layout settles before comparing.
            for (int i = 0; i < 4; i++)
            {
                SamplePixelsWithBlinkPhase(false);
            }

            Color[] pixelsWithCursorOff = SamplePixelsWithBlinkPhase(false);
            Color[] pixelsWithCursorOn = SamplePixelsWithBlinkPhase(true);

            // The helper text is drawn just below the TextBox's *own* bounds (border1.FinalRect.Bottom + 3
            // - border1 is inset from textBox's bottom by the reserved helperTextHeight Margin), not near
            // the bottom of the outer 100px-tall Border - textBox itself (Height=40, VerticalAlignment=Top)
            // sits at the top of that taller Border, leaving unused empty space below it.
            Rectangle textBoxRect = textBox.GetFinalRect();
            Rectangle borderRect = border.GetFinalRect();
            Rectangle helperTextBand = Rectangle.Intersect(
                new Rectangle(textBoxRect.Left, textBoxRect.Bottom - 15, textBoxRect.Width, 25),
                borderRect);

            for (int y = helperTextBand.Top; y < helperTextBand.Bottom; y++)
            {
                for (int x = helperTextBand.Left; x < helperTextBand.Right; x++)
                {
                    int index = y * renderTarget.Width + x;
                    Assert.AreEqual(pixelsWithCursorOff[index], pixelsWithCursorOn[index],
                        $"Expected the helper text region (row {y}, col {x}) to render identically regardless of the caret's blink phase - a mismatch means the caret's blink is scissor-clipping the helper text away on its 'on' phase, matching the reported bug.");
                }
            }
        }

        /// <summary>
        /// Reproduces a second, symmetric half of the same scissor-bleed bug: fixing
        /// DrawSelectionAndCursor's own clip (see HelperText_StaysVisible_RegardlessOfCursorBlinkPhase
        /// above) only re-widens the batch's live scissor rect back to layoutBounds on frames
        /// where that FillRect actually executes (cursorFlashOn &amp;&amp; hasFocus, or an active
        /// selection). On any frame where neither fires, the LAST ClipToRect call left standing is
        /// UIRenderer.DrawString's own narrowing (Rectangle.Intersect(layoutBounds, rectangle)) from
        /// the helper text's draw - and because SpriteSortMode.Deferred applies whatever scissor is
        /// live at EndBatch to the WHOLE batch, that narrow, helper-text-specific rect retroactively
        /// clipped away the underline (drawn near the input row's bottom) and the floating label
        /// (drawn near the top), both positioned well outside the helper text's own row - appearing
        /// as "the underline and the label both blink" in sync with the caret's "off" phase.
        /// Fixed by unconditionally resetting the clip back to layoutBounds as the very last
        /// operation in TextBoxTemplate.RenderControl's batch, regardless of which branches ran.
        /// </summary>
        [TestMethod]
        public void UnderlineAndFloatingLabel_StayVisible_RegardlessOfCursorBlinkPhase()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(host);

            var border = new Border
            {
                Width = 200,
                Height = 100,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(0f),
                BorderThickness = new Thickness(0),
                Background = Color.Transparent,
                EnableCaching = true,
            };
            host.AddChild(border);

            var textBox = new TextBox
            {
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Text = "ABCDEFGHIJKLMNOP",
                Label = "My Label",
                HelperText = "Helper text",
                Underline = true,
            };
            border.Content = textBox;

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            textBox.HasFocus.Value = true;
            textBox.CursorPosition = 4;

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            object template = textBox.Content;
            FieldInfo cursorFlashField = template.GetType().GetField("cursorFlashOn", BindingFlags.NonPublic | BindingFlags.Instance);

            Color[] SamplePixelsWithBlinkPhase(bool cursorFlashOn)
            {
                cursorFlashField.SetValue(template, cursorFlashOn);
                border.InvalidateCache();

                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Black);
                uiManager.Draw(spriteBatch, new GameTime(), renderTarget);
                graphicsDevice.SetRenderTarget(null);

                Color[] pixels = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(pixels);
                return pixels;
            }

            for (int i = 0; i < 4; i++)
            {
                SamplePixelsWithBlinkPhase(false);
            }

            Color[] pixelsWithCursorOff = SamplePixelsWithBlinkPhase(false);
            Color[] pixelsWithCursorOn = SamplePixelsWithBlinkPhase(true);

            // A column range on the far right of the 180px-wide TextBox - CursorPosition=4 puts
            // the caret's own 2px line near the left edge (just past 4 characters), nowhere near
            // here, so any difference found in this range can only be the underline/label
            // disappearing, never the caret's own legitimate blink.
            Rectangle textBoxRect = textBox.GetFinalRect();
            int farColumnLeft = textBoxRect.Right - 20;
            int farColumnRight = textBoxRect.Right;

            for (int y = textBoxRect.Top; y < textBoxRect.Bottom; y++)
            {
                for (int x = farColumnLeft; x < farColumnRight; x++)
                {
                    int index = y * renderTarget.Width + x;
                    Assert.AreEqual(pixelsWithCursorOff[index], pixelsWithCursorOn[index],
                        $"Expected the TextBox's right-hand region (row {y}, col {x}, far from the caret at CursorPosition=4) to render identically regardless of the caret's blink phase - a mismatch here means the underline/floating label are being scissor-clipped away on one of the caret's phases, matching the reported 'underline and label both blink' bug.");
                }
            }
        }

        /// <summary>
        /// Reproduces "the label doesn't animate from shrink to expanded and back - it jumps
        /// between the two states." Root cause: TextBoxTemplate's labelFontSize/labelColor/
        /// labelPosition/labelPunchAlpha (the four Binding&lt;T&gt; fields the floating label's
        /// shrink/expand transition eases via PropertyAnimationManager) are PRIVATE FIELDS, not
        /// public properties - invisible to UIComponent.EnsureBindingsWired's reflection scan
        /// (BindingFlags.Public only), so their Changed event was never subscribed by the general
        /// cache-invalidation-bubbling mechanism. PropertyAnimationManager.Update() (called every
        /// real frame from UIManager.Update, entirely independent of Draw-time caching) still
        /// correctly progresses each binding's .Value every frame - so the VALUE itself always
        /// eased smoothly - but with nothing bubbling that change to invalidate an ancestor
        /// Border's render cache, the only frames that ever got a chance to actually RE-RENDER
        /// (and thus visibly reflect the current progress) were whichever sparse, unrelated
        /// events happened to invalidate the cache anyway (e.g. the single frame HasFocus itself
        /// changes, a real public Binding that already bubbles correctly) - showing one frame
        /// near the animation's start and then, once some later unrelated event finally
        /// invalidates again (by which time the 80ms animation has long since finished
        /// internally), the fully-settled end state - "jumping" between exactly two renders
        /// instead of easing through every frame in between.
        /// </summary>
        [TestMethod]
        public void FloatingLabelAnimation_RendersMultipleDistinctIntermediateFrames_NotJustAJump()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(host);

            var border = new Border
            {
                Width = 200,
                Height = 100,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(0f),
                BorderThickness = new Thickness(0),
                Background = Color.Transparent,
                EnableCaching = true,
            };
            host.AddChild(border);

            // Empty Text (not ShrinkLabel-forced) so shrinkLabel tracks HasFocus alone -
            // focusing/unfocusing an empty box is exactly the classic Material floating-label
            // shrink/expand transition.
            var textBox = new TextBox
            {
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Text = "",
                Label = "My Label",
                HelperText = null,
            };
            border.Content = textBox;

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            long SampleChecksum()
            {
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Black);
                uiManager.Draw(spriteBatch, new GameTime(), renderTarget);
                graphicsDevice.SetRenderTarget(null);

                Color[] pixels = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(pixels);

                Rectangle rect = border.GetFinalRect();
                long checksum = 0;
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        Color c = pixels[y * renderTarget.Width + x];
                        checksum += c.R + (c.G * 31) + (c.B * 97);
                    }
                }
                return checksum;
            }

            // Settle the unfocused/expanded state and let labelAnimationInitialized's initial
            // snap happen.
            for (int i = 0; i < 4; i++)
            {
                SampleChecksum();
            }

            // Gain focus - this is a real, public Binding<bool> (HasFocus), so it already bubbles
            // an invalidation on its own, forcing the next RenderControl to compute the new
            // (shrunk) targets and kick off the 80ms AnimateTo transitions.
            textBox.HasFocus.Value = true;

            // No further explicit invalidation (no cursor move, no blink tick - not enough real
            // time passes for the 500ms blink interval) between these samples - only
            // uiManager.PerformLayout() (which calls PropertyAnimationManager.Update() every
            // time, per UIManager.Update) advances the animation clock. If the fix is working,
            // each of these should see the ancestor Border's cache invalidated again as the
            // labelPosition/labelFontSize/labelColor/labelPunchAlpha bindings progress toward
            // their shrunk targets, producing several DISTINCT consecutive checksums.
            long previousChecksum = SampleChecksum();
            int distinctTransitions = 0;
            for (int i = 0; i < 6; i++)
            {
                Thread.Sleep(15);
                uiManager.PerformLayout();
                long checksum = SampleChecksum();

                if (checksum != previousChecksum)
                {
                    distinctTransitions++;
                }

                previousChecksum = checksum;
            }

            Assert.IsTrue(distinctTransitions >= 3,
                $"Expected the floating label's shrink transition to render at least 3 distinct intermediate frames over ~90ms (an 80ms animation), but only saw {distinctTransitions} - too few distinct frames means the label is jumping between start/end states instead of easing smoothly, matching the reported bug.");
        }
    }
}
