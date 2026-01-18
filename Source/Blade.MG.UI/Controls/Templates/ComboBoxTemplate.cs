using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates;

public class ComboBoxTemplate : Control
{
    public Border RootBorder;
    public Label DisplayLabel;
    public Border HeaderBorder;
    public Rectangle HeaderRect => HeaderBorder?.FinalRect ?? FinalRect;
    public ListView ListView;
    public TextBox EditBox;

    protected override void InitTemplate()
    {
        base.InitTemplate();

        var combo = ParentAs<ComboBox>();

        // Root border contains header and the list
        RootBorder = new Border()
        {
            BorderColor = Color.Black,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(0),
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Top,
            Padding = new Thickness(4)
        };

        // Header (shows selected text or editable TextBox)
        HeaderBorder = new Border()
        {
            BorderColor = Color.Transparent,
            BorderThickness = new Thickness(0),
            Background = Color.Transparent,
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Top,
            Padding = new Thickness(6, 4, 6, 4)
        };

        // Label for non-editable state
        DisplayLabel = new Label()
        {
            Text = combo.Text, // binding in render
            Background = Color.Transparent,
            TextColor = Color.Black,
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Stretch
        };

        // TextBox for editable state
        EditBox = new TextBox()
        {
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Stretch,
            MaxLength = 250
        };

        //// ListView for dropdown items
        //ListView = new ListView()
        //{
        //    ItemTemplateType = typeof(ListViewItemTemplate),
        //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
        //    VerticalAlignment = VerticalAlignmentType.Stretch,
        //    Height = combo.DropDownHeight,
        //};

        // Compose header: if editable use EditBox else DisplayLabel.
        // Root content is a StackPanel-like setup: header then list
        var container = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignmentType.Stretch,
            VerticalAlignment = VerticalAlignmentType.Top,
            //Height = 120, // Temporary fixed height for testing
        };

        HeaderBorder.Content = combo.IsEditable.Value ? (UIComponent)EditBox : (UIComponent)DisplayLabel;
        container.AddChild(HeaderBorder);
        //container.AddChild(ListView);

        RootBorder.Content = container;
        Content = RootBorder;
        Content.Padding = new Thickness(4);
    }

    public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
    {
        var combo = ParentAs<ComboBox>();

        // Set the ListView data context to the filtered items (or full list if not editable)
        if (ListView != null)
        {
            var items = combo.GetFilteredItems();
            ListView.DataContext = items.Cast<object>().ToList();
        }

        // Control visibility of the list based on IsDropDownOpen
        if (ListView != null)
        {
            ListView.Visible.Value = combo.IsDropDownOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        //if (ListView.Visible.Value == Visibility.Visible)
        //{
        //    // Limit available height for the list when open (e.g. max 200px)
        //    //var maxListHeight = 200;
        //    //if (availableSize.Height > maxListHeight)
        //    //{
        //    //    availableSize.Height = maxListHeight;
        //    //}
        //}

        base.Measure(context, ref availableSize, ref parentMinMax);

        // nothing special - children will measure themselves
    }

    public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
    {
        var combo = ParentAs<ComboBox>();

        // Update header with proper content (edit vs label)
        if (HeaderBorder != null)
        {
            HeaderBorder.Content = combo.IsEditable.Value ? (UIComponent)EditBox : (UIComponent)DisplayLabel;
        }

        // Arrange using base
        base.Arrange(context, layoutBounds, parentLayoutBounds);

        // If editable, bind TextBox.Text to combo.Text
        if (EditBox != null)
        {
            EditBox.Text = combo.Text;
        }

        // Update label text when not editable or to reflect selected item
        if (DisplayLabel != null)
        {
            DisplayLabel.Text = combo.Text;
        }
    }

    public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
    {
        // Keep display text in sync before rendering
        var combo = ParentAs<ComboBox>();

        if (EditBox != null)
        {
            EditBox.Text = combo.Text;
        }

        if (DisplayLabel != null)
        {
            DisplayLabel.Text = combo.Text;
        }

        if (ListView != null)
        {
            ListView.DataContext = combo.GetFilteredItems().Cast<object>().ToList();
            ListView.Visible.Value = combo.IsDropDownOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        base.RenderControl(context, layoutBounds, parentTransform);
        //base.RenderControl(context, FinalContentRect, parentTransform);
        //base.RenderControl(context, ParentWindow.FinalContentRect, parentTransform);
    }

    // Forward hover/focus/selection state changes if needed (optional)
    public override async System.Threading.Tasks.Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
    {
        await base.HandleMouseClickEventAsync(uiWindow, uiEvent);
    }
}