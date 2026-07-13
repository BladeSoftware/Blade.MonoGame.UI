using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestScreenDesignerInTabPanelScenario
    {
        [TestMethod]
        public void Button_AddedAfterLayout_InsideTabPanelHostedScreenDesigner_ActuallyDrawsPixels()
        {
            // Same structure as TestScreenDesignerScenario.
            // Button_InFullNestedGridHierarchy_ActuallyDrawsPixels, but with the one layer that
            // test didn't have: in the real app (MainWindowUI), ScreenDesignerTab is never added
            // to the UIWindow directly - it's hosted as a TabPanel's active tab page
            // (mainWindowDockPanel.CenterPanel -> tabPanel -> AddTab(screenDesignerTab)). This
            // checks whether that extra hop (Container.AddChild via TabPanel.SetActiveTab,
            // rather than a direct UIWindow child) is what's silently breaking rendering for a
            // control added to the canvas after the initial layout pass.
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var tabPanel = new TabPanel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            ui.AddChild(tabPanel);
            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            // Mirrors ScreenDesignerTab.InitTemplate exactly.
            var outerGrid = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 40) });
            outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
            tabPanel.AddTab(outerGrid, "ScreenDesigner", setAsActiveTab: true);

            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            outerGrid.AddChild(toolbar, 0, 0);

            var contentGrid = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 170) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 320) });
            outerGrid.AddChild(contentGrid, 0, 1);

            var palette = new StackPanel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.AddChild(palette, 0, 0);

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

            uiManager.PerformLayout();

            // Mirrors AddControlFromPalette: added dynamically, after the initial layout pass has
            // already run once (same as clicking a palette button mid-session).
            var button = new Button { Text = "Button", Name = "BTN-TEST1" };
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

            Rectangle buttonRect = button.GetFinalRect();
            Point buttonCorner = new Point(buttonRect.Left + 3, buttonRect.Top + 3);
            Color pixelAtButtonCorner = pixels[buttonCorner.Y * 800 + buttonCorner.X];

            Point canvasBackgroundPoint = new Point(canvasHost.GetFinalRect().Right - 5, canvasHost.GetFinalRect().Bottom - 5);
            Color pixelAtCanvasBackground = pixels[canvasBackgroundPoint.Y * 800 + canvasBackgroundPoint.X];

            Assert.AreNotEqual(Rectangle.Empty, buttonRect, "Button has a zero/empty FinalRect - it never got laid out at all.");
            Assert.AreEqual(Color.Lime, pixelAtCanvasBackground, "Sanity check failed - canvasHost's own background isn't even rendering as expected.");
            Assert.AreNotEqual(Color.Lime, pixelAtButtonCorner, $"Expected something other than the bare canvas background at the button's corner {buttonCorner} (rect={buttonRect}) - got the canvas's own Lime, meaning nothing drew there at all.");
        }
    }
}
