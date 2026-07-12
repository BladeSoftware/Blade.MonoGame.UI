using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    [TestClass]
    public class TestListView
    {
        [TestMethod]
        public async Task CommitSelectionImmediately_False_ArrowHighlightsThenEnterCommits()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var items = new List<string> { "Alpha", "Beta", "Gamma" };

            var listView = new ListView
            {
                CommitSelectionImmediately = false
            };

            int selectionChangedCount = 0;
            listView.OnSelectionChanged = (item) => selectionChangedCount++;

            // Container.AddChild preserves an already-set DataContext rather than overwriting
            // it (see Container.AddChild) - this order (add first, assign after) still works,
            // but so does the reverse; see DataContext_SetBeforeAddChild_SurvivesAttach below,
            // which covers the order this used to require avoiding.
            ui.AddChild(listView);
            listView.DataContext = items;
            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            listView.HasFocus.Value = true;

            // Arrow navigation should only move the highlight, not commit a selection - this
            // is what lets ComboBox forward Up/Down to its dropdown list without the popup
            // closing on every keypress (OnSelectionChanged is what triggers the close).
            await listView.HandleKeyPressAsync(ui, new UIKeyEvent { Key = Keys.Down });

            Assert.AreEqual("Alpha", listView.HighlightedItem);
            Assert.IsNull(listView.SelectedItem);
            Assert.AreEqual(0, selectionChangedCount);

            // Enter commits the highlighted item.
            await listView.HandleKeyPressAsync(ui, new UIKeyEvent { Key = Keys.Enter });

            Assert.AreEqual("Alpha", listView.SelectedItem);
            Assert.IsNull(listView.HighlightedItem);
            Assert.AreEqual(1, selectionChangedCount);
        }

        [TestMethod]
        public async Task CommitSelectionImmediately_True_ArrowCommitsImmediately()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var items = new List<string> { "Alpha", "Beta", "Gamma" };

            // CommitSelectionImmediately defaults to true - unchanged behavior for standalone
            // ListView usage.
            var listView = new ListView();

            int selectionChangedCount = 0;
            listView.OnSelectionChanged = (item) => selectionChangedCount++;

            ui.AddChild(listView);
            listView.DataContext = items;
            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            listView.HasFocus.Value = true;

            await listView.HandleKeyPressAsync(ui, new UIKeyEvent { Key = Keys.Down });

            Assert.AreEqual("Alpha", listView.SelectedItem);
            Assert.IsNull(listView.HighlightedItem);
            Assert.AreEqual(1, selectionChangedCount);
        }

        [TestMethod]
        public async Task DataContext_SetBeforeAddChild_SurvivesAttach()
        {
            // ListView uses DataContext as its real items source (see ListView.DataContext
            // usage) - Container.AddChild used to unconditionally overwrite a child's
            // DataContext with its parent's the moment it was attached, which would silently
            // wipe out items assigned beforehand. Container.AddChild now preserves an
            // already-set DataContext instead, so this previously-unsafe order works too.
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var items = new List<string> { "Alpha", "Beta", "Gamma" };

            var listView = new ListView();
            listView.DataContext = items;
            ui.AddChild(listView);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            listView.HasFocus.Value = true;

            await listView.HandleKeyPressAsync(ui, new UIKeyEvent { Key = Keys.Down });

            Assert.AreEqual("Alpha", listView.SelectedItem);
        }
    }
}
