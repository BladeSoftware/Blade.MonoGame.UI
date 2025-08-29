using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class TreeNodeTemplate : Border
    {
        public Button button1;
        public Label label0;
        public Label label1;

        private Color backgroundNormal = new Color(Color.DarkSlateBlue, 0.55f);
        private Color backgroundHover = new Color(Color.SlateBlue, 0.80f);
        private Color backgroundFocused = new Color(Color.SlateBlue, 0.80f);

        private Color textColorNormal = Color.White;
        private Color textColorHover = Color.White;
        private Color textColorFocused = Color.White;


        private Color borderColorNormal = Color.Orange;

        private int borderThicknessNormal = 2;

        //private Binding<SpriteFont> SpriteFont { get; set; }

        public TreeNodeTemplate()
        {
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            IsHitTestVisible = true;

            //button = Parent as Button;
            ITreeNode treeNode = DataContext as ITreeNode;

            HorizontalAlignment = HorizontalAlignmentType.Stretch; //Parent.HorizontalAlignment;
            VerticalAlignment = VerticalAlignmentType.Stretch; //Parent.VerticalAlignment;
            //HorizontalContentAlignment = HorizontalAlignmentType.Left; //Parent.HorizontalContentAlignment;
            //VerticalContentAlignment = VerticalAlignmentType.Center; //Parent.VerticalContentAlignment;

            button1 = new Button()
            {
                TemplateType = typeof(ButtonBaseTemplate),

                //Text = ">",
                Width = 32,
                Height = 32,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
                TextColor = new Color(Color.Black, 1f),
                Background = Color.Transparent,

                CanHover = false,

                OnMouseDown = (sender, uiEvent) =>
                {
                    treeNode.IsExpanded = !treeNode.IsExpanded;
                    uiEvent.Handled = true;
                },

                //Padding = new Thickness(10),
                //OnClick = (p) =>
                //{
                //    treeNode.IsExpanded = !treeNode.IsExpanded;
                //    //p.Handled = true;
                //}
            };

            label0 = new Label()
            {
                Width = 16,
                Height = 32,
                //Text = ">",
                Text = treeNode?.Children != null && treeNode?.Children?.Count > 0 ? ">" : " ",
                TextColor = Theme.Tertiary, //Color.Black,
                Background = Color.Transparent,
                Margin = new Thickness(10, 0, 0, 0),

                //OnMouseDown = (sender, uiEvent) =>
                //{
                //    treeNode.IsExpanded = !treeNode.IsExpanded;
                //    uiEvent.Handled = true;
                //},

                //OnPrimaryClick = (sender, uiEvent) =>
                //{
                //    treeNode.IsExpanded = !treeNode.IsExpanded;
                //    uiEvent.Handled = true;
                //}

                //Transform = new Transform() with { Rotation = new Vector3(0, 0, 3.1415f / 4f) }
                //SpriteFont = button.SpriteFont // Use the Button Font
            };

            button1.Content = label0;

            label1 = new Label()
            {
                Text = treeNode?.Text ?? "null",
                TextColor = Color.Black,
                Background = Color.Transparent,
                //Width = 100,
                //Height = 100,
                Margin = new Thickness(4, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
                //HorizontalContentAlignment = HorizontalContentAlignment,
                //VerticalContentAlignment = VerticalContentAlignment,

                //SpriteFont = button.SpriteFont // Use the Button Font
            };

            //// Use the Parent Button's ContentAlignment values for the lable text placement
            //label1.HorizontalContentAlignment = HorizontalContentAlignment;
            //label1.VerticalContentAlignment = VerticalContentAlignment;


            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                //HorizontalContentAlignment = HorizontalAlignmentType.Left
            };

            //stackPanel.AddChild(label0, this);
            stackPanel.AddChild(button1, this);
            stackPanel.AddChild(label1, this);
            stackPanel.HorizontalScrollBarVisible = ScrollBarVisibility.Hidden;
            stackPanel.VerticalScrollBarVisible = ScrollBarVisibility.Hidden;
            stackPanel.Margin = new Thickness(4, 0);
            stackPanel.Padding = new Thickness(0, 4);


            Content = stackPanel;

            //this.OnClick = (sender, uiEvent) =>
            //{
            //    treeNode.IsExpanded = !treeNode.IsExpanded;
            //    uiEvent.Handled = true;
            //};

            OnMultiClick = (sender, uiEvent) =>
            {
                treeNode.IsExpanded = !treeNode.IsExpanded;
                uiEvent.Handled = true;
            };


        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            TreeNode treeNode = (TreeNode)DataContext;

            if (treeNode != null)
            {
                //if (treeNode.NodeType == ProjectNodeType.Project)
                //{
                //    if (this.MouseHover.Value)
                //    {
                //        this.Background = Color.Pink;
                //    }
                //}

                if (treeNode.IsExpanded)
                {
                    label0.Transform = label0.Transform with { Rotation = new Vector3(0, 0, 3.1415f / 4f) }; // Rotate 45 degrees
                }
                else
                {
                    label0.Transform = label0.Transform with { Rotation = new Vector3(0, 0, 0) };
                }
            }

            base.RenderControl(context, layoutBounds, parentTransform);
        }

        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            StateHasChanged();
        }


        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            //await base.HandleStateChangeAsync();

            TreeView parentTree = Parent as TreeView;
            TreeNode treeNode = DataContext as TreeNode;

            // Normal State
            //((StackPanel)Content).Background = Color.Transparent;
            Background = Color.Transparent;

            if (label1 == null)
            {
                return;
            }

            label1.Visible = Visibility.Visible;
            //editModeBorder.Visible = Visibility.Collapsed;

            if (treeNode == null)
            {
                return;
            }


            //if (MouseHover.Value)
            //{
            //    //((StackPanel)Content).Background = Color.LightGray;
            //    this.Background = Color.LightGray;
            //}
            //else
            //{
            //    //((StackPanel)Content).Background = Color.Transparent;
            //    this.Background = Color.Transparent;
            //}

            BorderThickness = 0;
            BorderColor = Theme.Outline; // Color.MidnightBlue;
            CornerRadius = 5;

            label1.TextColor = Theme.OnPrimaryContainer;

            if (treeNode.IsSelected && MouseHover.Value)
            {
                Background = Theme.SecondaryContainer; //Color.SlateBlue;
                BorderColor = Theme.Tertiary; // Color.MidnightBlue;
                BorderThickness = 2;

                label1.TextColor = Theme.OnSecondaryContainer;
            }
            else if (treeNode.IsSelected)
            {
                Background = Theme.SecondaryContainer; // Color.MediumSlateBlue;
                BorderThickness = 2;
                label1.TextColor = Theme.OnSecondaryContainer;

            }
            else if (MouseHover.Value)
            {
                BorderColor = Theme.Tertiary; // Color.MidnightBlue;
                BorderThickness = 2;
            }

        }

    }
}
