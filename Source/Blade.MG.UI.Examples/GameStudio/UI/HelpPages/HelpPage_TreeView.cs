using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Models;
using Examples.UI.Components;
using GameStudio;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Blade.MG.UI.Examples.GameStudio.UI.HelpPages
{
    public class HelpPage_TreeView : Panel
    {

        public HelpPage_TreeView()
        {

        }

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
                    Title = "Tree View"
                });



            var border = new Border()
            {
                BorderThickness = new Thickness(2),
                BorderColor = Color.Gray,
                Margin = new Thickness(20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                //Background = Color.HotPink,
                //Width = 800,
                Height = 600,
            };

            layoutPanel.AddChild(border);
            layoutPanel.StretchLastChild = true;


            var treeView = new TreeView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                ShowRootNode = true,
                //Text = new Binding<string>(() => "Label 2"),
                //TextColor = Color.White,
                //SpriteFont = fontArial8,
                Background = Color.White
            };
            //gridMainLayout.AddChild(treeView, 0, 0);


            TreeNode c1;
            TreeNode c2;
            TreeNode c3;

            int n = 0;

            TreeNode rootNode = new TreeNode { IsExpanded = true, Text = $"Root Node : n={++n}", Children = new List<ITreeNode>() };
            rootNode.Children.Add(c1 = new TreeNode { IsExpanded = true, Text = $"Child Node 1 : n={++n}", Children = new List<ITreeNode>() });
            c1.Children.Add(new TreeNode { IsExpanded = true, Text = $"Child Node 1 - A : n={++n}", Children = new List<ITreeNode>() });

            for (int i = 2; i < 20; i++)
            {
                rootNode.Children.Add(c1 = new TreeNode { IsExpanded = true, Text = $"Child Node {i} : n={++n}", Children = new List<ITreeNode>() });

                for (int j = 0; j < 3; j++)
                {
                    c1.Children.Add(c2 = new TreeNode { IsExpanded = true, Text = $"Sub Node {j} : n={++n}", Children = new List<ITreeNode>() });

                    for (int k = 0; k < 3; k++)
                    {
                        c2.Children.Add(c3 = new TreeNode { IsExpanded = true, Text = $"Sub-Sub Node {k} : n={++n}", Children = new List<ITreeNode>() });

                        for (int l = 0; l < 3; l++)
                        {
                            c3.Children.Add(new TreeNode { IsExpanded = true, Text = $"Sub-Sub-Sub Node {l} : n={++n}", Children = new List<ITreeNode>() });
                        }
                    }
                }

            }


            treeView.RootNode = rootNode;

            border.Content = treeView;


        }


    }
}