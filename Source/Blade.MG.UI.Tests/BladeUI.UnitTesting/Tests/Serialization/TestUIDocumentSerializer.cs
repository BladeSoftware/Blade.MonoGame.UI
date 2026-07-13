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
        public async Task SaveThenLoad_ControlAddedToReloadedRoot_StillGetsItsInternalTemplate()
        {
            // Reproduces a real bug: TemplateInitialised (has InitTemplate run yet - pure
            // runtime state) had no [JsonIgnore], so saving a design after it's been laid out
            // at least once baked "TemplateInitialised": true into the JSON. Reloading that
            // file then force-set true on every freshly-deserialized control before it was ever
            // parented, permanently skipping InitTemplate (and the internal template children -
            // border/label/chrome - it builds) for the whole tree. Layout/FinalRect still came
            // out perfectly correct (Measure/Arrange don't depend on it), so the only symptom
            // was controls being positioned but never actually rendering anything.
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var originalRoot = new StackPanel
            {
                Name = "Root",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                IsHitTestVisible = true,
            };

            // Attach to a real window and lay it out once, same as the Screen Designer does
            // before a user ever adds a control - this is what causes TemplateInitialised to
            // become true in memory prior to saving.
            ui.AddChild(originalRoot);
            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Assert.IsTrue(originalRoot.TemplateInitialised, "Test setup error - expected the root to have already gone through InitTemplate before saving.");

            string json = UIDocumentSerializer.SaveToJson(originalRoot);

            var reloadedRoot = (StackPanel)UIDocumentSerializer.LoadFromJson(json);

            var uiManager2 = new FakeUIManager();
            var ui2 = new EmptyUI();
            var canvasHost = new Panel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            ui2.AddChild(canvasHost);
            canvasHost.AddChild(reloadedRoot);
            uiManager2.AddUI(ui2);
            await uiManager2.PerformLayout();

            // Mirrors AddControlFromPalette: a fresh control added to the reloaded root.
            var button = new Button { Text = "Button" };
            reloadedRoot.AddChild(button);
            await uiManager2.PerformLayout();

            Assert.IsTrue(button.TemplateInitialised, "Button's own InitTemplate should have run.");
            Assert.IsTrue(button.PrivateControls.Any(), "Button has no internal template children - InitTemplate never actually built its visual chrome, even though TemplateInitialised is true.");
        }

        [TestMethod]
        public async Task SaveThenLoad_ChildAlreadyInTheSavedFile_StillGetsItsInternalTemplate()
        {
            // Distinct from SaveThenLoad_ControlAddedToReloadedRoot_StillGetsItsInternalTemplate
            // above, which only covers a control added *after* loading. This covers a control
            // that was already part of the saved design (reconstructed by LoadNode's recursive
            // Children loop, not by app code calling AddChild after the fact) - e.g. reopening a
            // .uiscreen file that already has a button in it from a previous session.
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var originalRoot = new StackPanel
            {
                Name = "Root",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                IsHitTestVisible = true,
            };
            var originalButton = new Button { Name = "BTN-TEST1", Text = "Button" };
            originalRoot.AddChild(originalButton);

            ui.AddChild(originalRoot);
            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Assert.IsTrue(originalButton.TemplateInitialised, "Test setup error - expected the button to have already gone through InitTemplate before saving.");

            string json = UIDocumentSerializer.SaveToJson(originalRoot);

            var reloadedRoot = (StackPanel)UIDocumentSerializer.LoadFromJson(json);
            var reloadedButton = (Button)reloadedRoot.Children.Single();

            var uiManager2 = new FakeUIManager();
            var ui2 = new EmptyUI();
            var canvasHost = new Panel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            ui2.AddChild(canvasHost);
            canvasHost.AddChild(reloadedRoot);
            uiManager2.AddUI(ui2);
            await uiManager2.PerformLayout();

            Assert.IsTrue(reloadedButton.TemplateInitialised, "The button that was already in the saved file should have had its own InitTemplate run once the reloaded tree was attached to a window.");
            Assert.IsTrue(reloadedButton.PrivateControls.Any(), "Button has no internal template children - InitTemplate never actually built its visual chrome, even though TemplateInitialised is true.");
        }

        [TestMethod]
        public void LoadFromJson_IgnoresStaleTemplateInitialisedField_FromAnOlderSavedFile()
        {
            // A hand-crafted document mimicking a .uiscreen file saved by a build from before
            // TemplateInitialised got [JsonIgnore] - such files still have the field baked in on
            // disk and will keep being loaded going forward. LoadNode must ignore it (matching
            // Save's own filter) rather than reapplying it just because FindMember can still
            // resolve the property by name via reflection.
            string json = """
            {
                "Root": {
                    "Type": "StackPanel",
                    "Name": "Root",
                    "Properties": {
                        "TemplateInitialised": true
                    }
                }
            }
            """;

            var loaded = (StackPanel)UIDocumentSerializer.LoadFromJson(json);

            Assert.IsFalse(loaded.TemplateInitialised, "A freshly-deserialized component must start with TemplateInitialised=false regardless of what an older file's JSON says, so its own InitTemplate still runs once attached to a parent.");
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
