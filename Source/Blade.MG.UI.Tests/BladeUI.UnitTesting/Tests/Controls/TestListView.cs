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

            // AddChild(item, parent, dataContext) overwrites item.DataContext with its own
            // dataContext parameter (default null) when called without one - see
            // Container.AddChild - so DataContext must be set after adding, not via an object
            // initializer beforehand.
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
    }
}
