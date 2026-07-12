using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Serialization;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BladeUI.UnitTesting.Tests.Serialization
{
    public class TestSettingsViewModel
    {
        public string PlayerName { get; set; } = "";
    }

    [TestClass]
    public class TestUIDocumentSerializer
    {
        [TestMethod]
        public async Task SaveThenLoad_RoundTripsLiteralAndBoundProperties()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var originalPanel = new Panel
            {
                Name = "SettingsPanel",
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            var originalCheckBox = new CheckBox
            {
                Name = "SoundEnabled",
                Text = "Enable Sound",
                IsChecked = true,
            };

            originalPanel.AddChild(originalCheckBox);

            ui.AddChild(originalPanel);
            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            // --- Save ---
            UIDocument document = UIDocumentSerializer.Save(originalPanel);

            Assert.AreEqual("Panel", document.Root.Type);
            Assert.AreEqual("SettingsPanel", document.Root.Name);
            Assert.IsNotNull(document.Root.Children);
            Assert.AreEqual(1, document.Root.Children.Count);
            Assert.AreEqual("CheckBox", document.Root.Children[0].Type);
            Assert.AreEqual("SoundEnabled", document.Root.Children[0].Name);
            Assert.AreEqual("Enable Sound", document.Root.Children[0].Properties["Text"].GetString());

            // Simulate a hand-authored (or future designer-authored) edit that binds the
            // CheckBox's Text to a named path instead of a literal, proving a document can mix
            // literal and bound properties on the same node.
            document.Root.Children[0].Properties["Text"] =
                JsonSerializer.SerializeToElement(new Dictionary<string, string> { ["$bind"] = "PlayerName" });

            // Round-trip through actual JSON text too, not just the in-memory UIDocument.
            string json = JsonSerializer.Serialize(document);
            UIDocument reloadedDocument = JsonSerializer.Deserialize<UIDocument>(json);

            // --- Load, against a fresh view-model as DataContext ---
            var viewModel = new TestSettingsViewModel { PlayerName = "Ada" };
            var loadedRoot = (Panel)UIDocumentSerializer.Load(reloadedDocument, viewModel);

            Assert.AreEqual("SettingsPanel", loadedRoot.Name);
            Assert.AreEqual(1, loadedRoot.Children.Count);

            var loadedCheckBox = (CheckBox)loadedRoot.Children[0];
            Assert.AreEqual("SoundEnabled", loadedCheckBox.Name);
            Assert.AreEqual(true, loadedCheckBox.IsChecked.Value);

            // $bind round-trip: reading Text pulls live from the view-model...
            Assert.AreEqual("Ada", loadedCheckBox.Text.Value);

            // ...and writing through the Binding writes back to the view-model.
            loadedCheckBox.Text.Value = "Grace";
            Assert.AreEqual("Grace", viewModel.PlayerName);

            // FindByName locates the same control post-load, for code-side event wiring.
            Assert.AreSame(loadedCheckBox, loadedRoot.FindByName("SoundEnabled"));
        }

        [TestMethod]
        public void SaveThenLoad_RoundTripsTabPanelAndDockPanel()
        {
            // TabPanel/DockPanel don't expose their real content through the base Children
            // collection (TabPanel.AddChild even throws) - this proves the dedicated BuildNode/
            // LoadNode branches for both actually preserve content that the generic Container
            // walk would otherwise lose or crash on.
            var tabPanel = new TabPanel { Name = "SettingsTabs" };
            var dockPanel = new DockPanel { Name = "SettingsDock" };

            var root = new Panel { Name = "Root" };
            // TabPanel must be attached to its parent before AddTab is called - Container.
            // AddChild overwrites a child's DataContext with the parent's, and TabPanel
            // repurposes DataContext as its own tab-list storage (see TabPanel.TabPages), so
            // attaching afterward would silently wipe out any tabs added first. This is also
            // the order MainWindowUI.cs itself already uses for exactly this reason.
            root.AddChild(tabPanel);
            root.AddChild(dockPanel);

            tabPanel.AddTab(new Label { Name = "GeneralLabel", Text = "General settings" }, "General");
            tabPanel.AddTab(new Label { Name = "AdvancedLabel", Text = "Advanced settings" }, "Advanced");

            dockPanel.LeftPanel.AddChild(new Label { Name = "NavLabel", Text = "Navigation" });
            dockPanel.CenterPanel.AddChild(new Button { Name = "ApplyButton", Text = "Apply" });

            string json = UIDocumentSerializer.SaveToJson(root);
            var loadedRoot = (Panel)UIDocumentSerializer.LoadFromJson(json);

            Assert.AreEqual(2, loadedRoot.Children.Count);

            var loadedTabPanel = (TabPanel)loadedRoot.Children[0];
            var tabPages = loadedTabPanel.TabPages.ToList();
            Assert.AreEqual(2, tabPages.Count);
            Assert.AreEqual("General", tabPages[0].Header);
            Assert.AreEqual("General settings", ((Label)tabPages[0].Content).Text.Value);
            Assert.AreEqual("Advanced", tabPages[1].Header);
            Assert.AreEqual("Advanced settings", ((Label)tabPages[1].Content).Text.Value);

            var loadedDockPanel = (DockPanel)loadedRoot.Children[1];
            Assert.AreEqual(1, loadedDockPanel.LeftPanel.Children.Count);
            Assert.AreEqual("Navigation", ((Label)loadedDockPanel.LeftPanel.Children[0]).Text.Value);
            Assert.AreEqual(1, loadedDockPanel.CenterPanel.Children.Count);
            Assert.AreEqual("Apply", ((Button)loadedDockPanel.CenterPanel.Children[0]).Text.Value);
            Assert.AreEqual(0, loadedDockPanel.RightPanel.Children.Count);
        }
    }
}
