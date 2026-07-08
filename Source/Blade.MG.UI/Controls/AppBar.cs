using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Blade.MG.UI.Controls
{
    /// <summary>
    /// A Material-style top app bar: a themed horizontal strip with a title on the left and a
    /// row of trailing actions (typically <see cref="IconButton"/>s) on the right. Intended to
    /// sit in a window's top region (e.g. DockPanel.TopPanel).
    /// </summary>
    public class AppBar : Control
    {
        private Grid grid;
        private Label titleLabel;
        private StackPanel actionsPanel;

        // AddAction is commonly called as part of building an AppBar (object-initializer style)
        // before it's ever parented, i.e. before InitTemplate has created actionsPanel - buffer
        // those calls and flush them once the template exists instead of requiring callers to
        // add the AppBar to the tree first.
        private List<UIComponent> pendingActions;

        public Binding<string> Title { get; set; } = new Binding<string>("");

        private Binding<Color> titleColor = new Binding<Color>();
        public Binding<Color> TitleColor { get => titleColor; set => SetField(ref titleColor, value); }

        public AppBar()
        {
            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Top;

            Height = 48;
            Padding = new Thickness(16, 0);

            IsHitTestVisible = true;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

            titleLabel = new Label
            {
                Text = Title,
                TextColor = TitleColor,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
            };
            grid.AddChild(titleLabel, 0, 0);

            actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignmentType.Right,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            grid.AddChild(actionsPanel, 1, 0);

            Content = grid;

            if (pendingActions != null)
            {
                foreach (var action in pendingActions)
                {
                    actionsPanel.AddChild(action);
                }

                pendingActions = null;
            }
        }

        /// <summary>
        /// Adds a trailing action (e.g. an <see cref="IconButton"/>) to the right side of the bar.
        /// Safe to call before the AppBar has been added to the tree.
        /// </summary>
        public void AddAction(UIComponent action)
        {
            if (actionsPanel != null)
            {
                actionsPanel.AddChild(action);
                return;
            }

            pendingActions ??= new List<UIComponent>();
            pendingActions.Add(action);
        }

        protected override void HandleStateChange()
        {
            ApplyThemedValue(this, Background, nameof(Background), Theme.SurfaceVariant);

            if (titleLabel != null)
            {
                ApplyThemedValue(this, titleLabel.TextColor, nameof(TitleColor), Theme.OnSurfaceVariant);
            }
        }
    }
}
