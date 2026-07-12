using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestScreenDesignerScenario
    {
        [TestMethod]
        public void Button_AddedToVerticalStackPanel_InsideStretchedPanel_HasNonZeroFinalRect()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            // Mirrors ScreenDesignerTab.BuildCanvasHost's canvasHost
            var canvasHost = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(canvasHost);

            // Mirrors ScreenDesignerTab.NewDesignRootContainer's designRoot
            var designRoot = new StackPanel
            {
                Name = "Root",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                IsHitTestVisible = true,
                Padding = new Thickness(8),
            };
            canvasHost.AddChild(designRoot);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            // Mirrors PaletteControls' Button factory, added AFTER the initial layout pass -
            // i.e. dynamically, the same way AddControlFromPalette does it.
            var button = new Button { Text = "Button" };
            designRoot.AddChild(button);

            uiManager.PerformLayout();

            Rectangle buttonRect = button.GetFinalRect();

            Assert.AreEqual(Visibility.Visible, button.Visible.Value);
            Assert.IsTrue(buttonRect.Width > 0, $"Expected a non-zero width, got {buttonRect}");
            Assert.IsTrue(buttonRect.Height > 0, $"Expected a non-zero height, got {buttonRect}");
        }

        [TestMethod]
        public void Button_InFullNestedGridHierarchy_MatchingScreenDesignerTab_HasSaneFinalRect()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            // Mirrors ScreenDesignerTab.InitTemplate's outerGrid exactly.
            var outerGrid = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 40) });
            outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
            ui.AddChild(outerGrid);

            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            outerGrid.AddChild(toolbar, 0, 0);

            // Mirrors ScreenDesignerTab.InitTemplate's contentGrid exactly (170px | * | 320px).
            var contentGrid = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 170) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 320) });
            outerGrid.AddChild(contentGrid, 0, 1);

            var palette = new StackPanel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.AddChild(palette, 0, 0);

            var canvasHost = new Panel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.AddChild(canvasHost, 1, 0);

            var inspectorColumn = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.AddChild(inspectorColumn, 2, 0);

            var designRoot = new StackPanel
            {
                Name = "Root",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                IsHitTestVisible = true,
                Padding = new Thickness(8),
            };
            canvasHost.AddChild(designRoot);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            var button = new Button { Text = "Button" };
            designRoot.AddChild(button);

            uiManager.PerformLayout();

            Rectangle canvasHostRect = canvasHost.GetFinalRect();
            Rectangle buttonRect = button.GetFinalRect();

            // The button must land inside canvasHost's own bounds (the visible design surface),
            // not off to one side of it (e.g. bleeding into the palette or inspector columns).
            Assert.IsTrue(canvasHostRect.Contains(buttonRect), $"canvasHost={canvasHostRect} does not contain button={buttonRect}");
            Assert.IsTrue(buttonRect.Width > 0 && buttonRect.Height > 0, $"Expected a non-zero size, got {buttonRect}");
        }

        [TestMethod]
        public void Button_InFullNestedGridHierarchy_ActuallyDrawsPixels()
        {
            // The layout-only tests above prove positioning is correct, but that doesn't prove
            // anything actually gets drawn to the screen - RenderControl is a separate code path
            // from Measure/Arrange. This renders a real frame to an off-screen target and reads
            // the pixels back, to rule out (or confirm) a render-specific bug that a pure layout
            // assertion can't see.
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var outerGrid = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 40) });
            outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
            ui.AddChild(outerGrid);

            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            outerGrid.AddChild(toolbar, 0, 0);

            var contentGrid = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 170) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 320) });
            outerGrid.AddChild(contentGrid, 0, 1);

            var palette = new StackPanel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.AddChild(palette, 0, 0);

            // A plain, distinct, opaque background - no texture/tiling, so any pixel that isn't
            // this exact color at the button's location proves something else drew there.
            var canvasHost = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Background = Color.Lime,
            };
            contentGrid.AddChild(canvasHost, 1, 0);

            var inspectorColumn = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.AddChild(inspectorColumn, 2, 0);

            var designRoot = new StackPanel
            {
                Name = "Root",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                IsHitTestVisible = true,
                Padding = new Thickness(8),
            };
            canvasHost.AddChild(designRoot);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            // Note: Button is a TemplatedControl - its internal template (ButtonTemplate) draws
            // its own themed chrome over this raw Background, so this Red never actually shows
            // up as a solid fill (confirmed empirically - see the comment below). Still useful
            // to set: it proves the template draws *something* opaque, not nothing.
            var button = new Button { Text = "Button", Background = Color.Red };
            designRoot.AddChild(button);

            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 800, 600);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Black);

            uiManager.Draw(spriteBatch, new GameTime(), renderTarget);

            graphicsDevice.SetRenderTarget(null);

            Color[] pixels = new Color[800 * 600];
            renderTarget.GetData(pixels);

            // Sample near a corner, not dead-center - the button's own "Button" text glyphs are
            // centered and would otherwise be sampled instead of its template's chrome fill.
            Rectangle buttonRect = button.GetFinalRect();
            Point buttonCorner = new Point(buttonRect.Left + 3, buttonRect.Top + 3);
            Color pixelAtButtonCorner = pixels[buttonCorner.Y * 800 + buttonCorner.X];

            Point canvasBackgroundPoint = new Point(canvasHost.GetFinalRect().Right - 5, canvasHost.GetFinalRect().Bottom - 5);
            Color pixelAtCanvasBackground = pixels[canvasBackgroundPoint.Y * 800 + canvasBackgroundPoint.X];

            Assert.AreEqual(Color.Lime, pixelAtCanvasBackground, "Sanity check failed - canvasHost's own background isn't even rendering as expected, something more fundamental is broken.");
            Assert.AreNotEqual(Color.Lime, pixelAtButtonCorner, $"Expected something other than the bare canvas background at the button's corner {buttonCorner} - got the canvas's own Lime, meaning nothing drew there at all.");
        }
    }
}
