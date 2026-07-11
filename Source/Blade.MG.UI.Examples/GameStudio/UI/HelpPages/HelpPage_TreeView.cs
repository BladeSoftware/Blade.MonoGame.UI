using Blade.MG.UI;
using Microsoft.Xna.Framework;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Models;
using Examples.UI.Components;
using System.Collections.Generic;
using System.Linq;

namespace Examples.UI.HelpPages
{
    public class HelpPage_TreeView : Panel
    {
        protected override void InitTemplate()
        {
            base.InitTemplate();

            var layoutPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            base.AddChild(layoutPanel);

            layoutPanel.AddChild(
                new PageHeader()
                {
                    Padding = new Thickness(30, 0, 0, 0),
                    Title = "Tree View",
                    Description = "A virtualized, expandable tree - only visible rows are measured and drawn. " +
                        "Click a row, then use Up/Down to move between rows, Left/Right to collapse/expand, " +
                        "and Enter/Space to toggle expansion of the selected row."
                });

            var treeView = new TreeView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                ShowRootNode = true,
                RootNode = BuildProjectTree(),
            };

            var selectedLabel = new Label()
            {
                Padding = new Thickness(30, 0, 0, 0),
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 14,
                TextColor = new Binding<Color>(() => Theme.OnSurfaceVariant),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                Text = "Selected: (none)",
            };

            treeView.OnSelectedNodeChanged = node =>
            {
                selectedLabel.Text.Value = $"Selected: {node?.Text ?? "(none)"}";
            };

            layoutPanel.AddChild(selectedLabel);

            var border = new Border()
            {
                BorderThickness = new Thickness(1),
                BorderColor = new Binding<Color>(() => Theme.Outline),
                Margin = new Thickness(30, 10, 30, 20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Height = 500,
            };

            layoutPanel.AddChild(border);
            layoutPanel.StretchLastChild = true;

            border.Content = treeView;
        }

        // A mock project layout - stands in for the kind of file/folder tree a real
        // application would bind here, while still giving TreeView a few dozen nodes to
        // virtualize.
        private static TreeNode BuildProjectTree()
        {
            TreeNode Folder(string name, params TreeNode[] children) => new TreeNode
            {
                IsExpanded = true,
                Text = name,
                Children = children.Cast<ITreeNode>().ToList(),
            };

            TreeNode File(string name) => new TreeNode { IsExpanded = false, Text = name, Children = new List<ITreeNode>() };

            return Folder("Blade.MG.UI",
                Folder("Controls",
                    File("Button.cs"), File("CheckBox.cs"), File("ComboBox.cs"), File("Grid.cs"),
                    File("Label.cs"), File("ListView.cs"), File("Panel.cs"), File("TabPanel.cs"),
                    File("TextBox.cs"), File("TreeView.cs")),
                Folder("Theming",
                    File("UITheme.cs"), File("DefaultThemes.cs")),
                Folder("Components",
                    File("Binding.cs"), File("UIComponent.cs"), File("UIComponentDrawable.cs")),
                Folder("Examples",
                    Folder("HelpPages",
                        File("HelpPage_Button.cs"), File("HelpPage_Grid.cs"), File("HelpPage_Label.cs"),
                        File("HelpPage_TreeView.cs"), File("HelpPage_TabPanel.cs")),
                    Folder("Components",
                        File("PageHeader.cs"), File("Section.cs"))));
        }

    }
}
