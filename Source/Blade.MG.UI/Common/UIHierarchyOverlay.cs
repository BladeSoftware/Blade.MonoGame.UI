using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Blade.MG.UI.Common
{
    public class UIHierarchyOverlay : UIWindow
    {
        private TreeView treeView;

        public override void Initialize(Game game)
        {
            IsHitTestVisible = true;

            base.Initialize(game);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            //BuildScreen();

            // Start off hidden
            Visible.Value = Visibility.Hidden;

        }

        private void BuildScreen()
        {
            var uiManager = Game.Services.GetService<UIManager>();


            // Create the TreeView
            treeView = new TreeView
            {
                Name = "UIHierarchyTreeView",
                Margin = new Thickness(8),
                Padding = new Thickness(4),
                Background = new Color(Color.Gray, 1f),
                VerticalAlignment = VerticalAlignmentType.Top,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                Width = 400,
                Height = 600,
                ShowRootNode = false
            };

            // Build the tree nodes from UIManager
            if (uiManager != null)
            {
                var root = new TreeNode
                {
                    Text = "UI Manager",
                    IsExpanded = true
                };

                var rootNodes = new List<TreeNode>();

                foreach (var window in uiManager.GetWindows)
                {
                    var windowNode = new TreeNode
                    {
                        Text = window.Name ?? window.GetType().Name,
                        IsExpanded = true
                    };
                    BuildControlTree(window, windowNode);
                    rootNodes.Add(windowNode);
                }

                root.AddRange(rootNodes);

                treeView.RootNode = root;
            }

            AddChild(treeView);
        }

        private void BuildControlTree(UIComponent component, TreeNode parentNode)
        {
            if (component is Container container && container.Children != null)
            {
                foreach (var child in container.Children)
                {
                    var childNode = new TreeNode
                    {
                        Text = (child.Name ?? child.GetType().Name) + $" [{child.FinalRect}]",
                        IsExpanded = true
                    };

                    parentNode.Children = parentNode.Children ?? new List<ITreeNode>();
                    parentNode.Children.Add(childNode);
                    BuildControlTree(child, childNode);
                }
            }
            else if (component is Control contentControl && contentControl.Content != null)
            {
                var content = contentControl.Content;
                var contentNode = new TreeNode
                {
                    Text = (content.Name ?? content.GetType().Name) + $" [{content.FinalRect}]",
                    IsExpanded = true
                };
                parentNode.Children = parentNode.Children ?? new List<ITreeNode>();
                parentNode.Children.Add(contentNode);
                BuildControlTree(content, contentNode);
            }
        }


        public override void RenderLayout(GameTime gameTime)
        {
            base.RenderLayout(gameTime);
        }

        public override Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            // Toggle overlay with F12
            if (uiEvent.Key == Microsoft.Xna.Framework.Input.Keys.F12)
            {
                Visible.Value = Visible.Value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

                if (Visible.Value == Visibility.Visible)
                {
                    RemoveAllChildren();
                    BuildScreen();
                }

                uiEvent.Handled = true;
                return Task.CompletedTask;
            }
            return base.HandleKeyPressAsync(uiWindow, uiEvent);
        }

        public override Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (Visible.Value == Visibility.Visible)
            {
                uiEvent.Handled = true;
            }

            return base.HandleMouseClickEventAsync(uiWindow, uiEvent);
        }

        public override Task HandleMouseRightClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (Visible.Value == Visibility.Visible)
            {
                uiEvent.Handled = true;
            }

            return base.HandleMouseRightClickEventAsync(uiWindow, uiEvent);
        }

    }
}