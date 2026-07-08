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

        private Binding<Color> textColor = new Binding<Color>();
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

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

            //Margin = Margin.Value with { Left = 12 };

            button1 = new Button()
            {
                TemplateType = typeof(ButtonBaseTemplate),

                //Text = ">",
                Width = 32,
                Height = 32,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
                TextColor = Theme.OnSurface,
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

                // Link to this template's own TextColor binding so a subclass/customizer can
                // override it directly as well as via SetStyleOverride.
                TextColor = TextColor,
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
            TreeView parentTree = Parent as TreeView;
            TreeNode treeNode = DataContext as TreeNode;

            // Normal State
            ApplyThemedValue(this, Background, nameof(Background), Color.Transparent);

            if (label1 == null)
            {
                return;
            }

            label1.Visible = Visibility.Visible;

            if (treeNode == null)
            {
                return;
            }

            ApplyThemedValue(this, BorderThickness, nameof(BorderThickness), new Thickness(0));
            ApplyThemedValue(this, BorderColor, nameof(BorderColor), Theme.Outline);
            ApplyThemedValue(this, CornerRadius, nameof(CornerRadius), new CornerRadius(8));
            ApplyThemedValue(this, label1.TextColor, nameof(TreeNodeTemplate.TextColor), Theme.OnPrimaryContainer);

            if (treeNode.IsSelected && MouseHover.Value)
            {
                ApplyThemedValue(this, Background, nameof(Background), Theme.SecondaryContainer);
                ApplyThemedValue(this, BorderColor, nameof(BorderColor), Theme.Tertiary);
                ApplyThemedValue(this, BorderThickness, nameof(BorderThickness), new Thickness(2));
                ApplyThemedValue(this, label1.TextColor, nameof(TreeNodeTemplate.TextColor), Theme.OnSecondaryContainer);
            }
            else if (treeNode.IsSelected)
            {
                ApplyThemedValue(this, Background, nameof(Background), Theme.SecondaryContainer);
                ApplyThemedValue(this, BorderThickness, nameof(BorderThickness), new Thickness(2));
                ApplyThemedValue(this, label1.TextColor, nameof(TreeNodeTemplate.TextColor), Theme.OnSecondaryContainer);
            }
            else if (MouseHover.Value)
            {
                ApplyThemedValue(this, BorderColor, nameof(BorderColor), Theme.Tertiary);
                ApplyThemedValue(this, BorderThickness, nameof(BorderThickness), new Thickness(2));
            }
        }

    }
}
