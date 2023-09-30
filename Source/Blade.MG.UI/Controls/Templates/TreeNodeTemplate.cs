using Blade.UI.Components;
using Blade.UI.Events;
using Blade.UI.Models;
using Microsoft.Xna.Framework;

namespace Blade.UI.Controls.Templates
{
    public class TreeNodeTemplate : Border
    {
        //public Button button1;
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

            HitTestVisible = true;

            //button = Parent as Button;
            ITreeNode treeNode = (ITreeNode)DataContext;

            this.HorizontalAlignment = HorizontalAlignmentType.Stretch; //Parent.HorizontalAlignment;
            this.VerticalAlignment = VerticalAlignmentType.Stretch; //Parent.VerticalAlignment;
            this.HorizontalContentAlignment = HorizontalAlignmentType.Left; //Parent.HorizontalContentAlignment;
            this.VerticalContentAlignment = VerticalAlignmentType.Center; //Parent.VerticalContentAlignment;

            //button1 = new Button()
            //{
            //    Text = ">",
            //    Width = 20,
            //    Height = 20,
            //    HorizontalAlignment = HorizontalAlignmentType.Left,
            //    VerticalAlignment = VerticalAlignmentType.Center,
            //    FontColor = new Color(Color.Black, 1f),
            //    Background = Color.Red,
            //    //Padding = new Thickness(10),
            //    HorizontalContentAlignment = HorizontalAlignmentType.Left,
            //    //OnClick = (p) =>
            //    //{
            //    //    treeNode.IsExpanded = !treeNode.IsExpanded;
            //    //    //p.Handled = true;
            //    //}
            //};

            label0 = new Label()
            {
                Width = 16,
                Height = 32,
                //Text = ">",
                Text = (treeNode?.Children != null && treeNode?.Children?.Count > 0) ? ">" : " ",
                TextColor = Theme.Tertiary, //Color.Black,
                Background = Color.Transparent,
                Margin = new Thickness(10, 0, 0, 0),

                OnClick = (sender, uiEvent) =>
                {
                    treeNode.IsExpanded = !treeNode.IsExpanded;
                    uiEvent.Handled = true;
                }

                //Transform = new Transform() with { Rotation = new Vector3(0, 0, 3.1415f / 4f) }
                //SpriteFont = button.SpriteFont // Use the Button Font
            };

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
                HorizontalContentAlignment = HorizontalContentAlignment,
                VerticalContentAlignment = VerticalContentAlignment,

                //SpriteFont = button.SpriteFont // Use the Button Font
            };

            //// Use the Parent Button's ContentAlignment values for the lable text placement
            //label1.HorizontalContentAlignment = HorizontalContentAlignment;
            //label1.VerticalContentAlignment = VerticalContentAlignment;


            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignmentType.Left
            };

            //stackPanel.AddChild(button1, this);
            stackPanel.AddChild(label0, this);
            stackPanel.AddChild(label1, this);
            stackPanel.HorizontalScrollBarVisible = false;
            stackPanel.VerticalScrollBarVisible = false;
            stackPanel.Margin = new Thickness(4, 0);
            stackPanel.Padding = new Thickness(0, 4);


            Content = stackPanel;

            //this.OnClick = (sender, uiEvent) =>
            //{
            //    treeNode.IsExpanded = !treeNode.IsExpanded;
            //    uiEvent.Handled = true;
            //};

            this.OnDoubleClick = (sender, uiEvent) =>
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
            this.Background = Color.Transparent;

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

            this.BorderThickness = 0;
            this.BorderColor = Theme.Outline; // Color.MidnightBlue;
            this.CornerRadius = 5;

            label1.TextColor = Theme.OnPrimaryContainer;

            if (treeNode.IsSelected && MouseHover.Value)
            {
                this.Background = Theme.SecondaryContainer; //Color.SlateBlue;
                this.BorderColor = Theme.Tertiary; // Color.MidnightBlue;
                this.BorderThickness = 2;

                label1.TextColor = Theme.OnSecondaryContainer;
            }
            else if (treeNode.IsSelected)
            {
                this.Background = Theme.SecondaryContainer; // Color.MediumSlateBlue;
                this.BorderThickness = 2;
                label1.TextColor = Theme.OnSecondaryContainer;

            }
            else if (MouseHover.Value)
            {
                this.BorderColor = Theme.Tertiary; // Color.MidnightBlue;
                this.BorderThickness = 2;
            }

        }

    }
}
