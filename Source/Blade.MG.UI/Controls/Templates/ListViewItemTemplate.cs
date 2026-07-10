using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Controls.Templates
{
    public class ListViewItemTemplate : Control, IItemTemplate
    {
        public Label label1;

        private Binding<Color> textColor = new Binding<Color>();
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        private Binding<bool> isSelected = new Binding<bool>(false);
        public Binding<bool> IsSelected { get => isSelected; set => SetField(ref isSelected, value); }

        [JsonIgnore]
        [XmlIgnore]
        private Binding<SpriteFont> SpriteFont { get; set; }

        public ListViewItemTemplate()
        {
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            IsHitTestVisible = true;

            // Let focus land on the owning ListView (see ListView.InitTemplate) rather than
            // the individual item - SelectFirst's mouse-down focus search picks the deepest
            // CanFocus match under the cursor, and ListView's own keyboard navigation is
            // gated on its HasFocus, not any one item's.
            CanFocus = false;

            string item = DataContext?.ToString() ?? "null";

            //this.HorizontalAlignment = Parent.HorizontalContentAlignment;
            //this.VerticalAlignment = Parent.VerticalContentAlignment;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;
            //HorizontalContentAlignment = HorizontalAlignmentType.Left; //Parent.HorizontalContentAlignment;
            //VerticalContentAlignment = VerticalAlignmentType.Center; //Parent.VerticalContentAlignment;

            label1 = new Label()
            {
                Height = "40px",
                Text = item,

                // Link to this template's own TextColor binding so a subclass/customizer can
                // override it directly as well as via SetStyleOverride.
                TextColor = TextColor,
                Background = Color.Transparent,
                //Width = 100,
                //Height = 100,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(8, 0, 0, 0),

                //SpriteFont = button.SpriteFont // Use the Button Font
            };

            // Use the Parent Button's ContentAlignment values for the lable text placement
            //label1.HorizontalContentAlignment = HorizontalContentAlignment;
            //label1.VerticalContentAlignment = VerticalContentAlignment;


            Content = label1;


        }

        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            HasFocus = uiEvent.Focused;

            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            MouseHover = uiEvent.Hover;

            await base.HandleHoverChangedAsync(uiWindow, uiEvent);
            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            var content = (UIComponentDrawable)Content;

            // Normal State
            ApplyThemedValue(this, content.Background, nameof(Background), Color.Transparent);
            ApplyThemedValue(this, label1.TextColor, nameof(TextColor), Theme.OnSurface);

            // Selected State
            if (IsSelected.Value)
            {
                ApplyThemedValue(this, content.Background, nameof(Background), Theme.PrimaryContainer);
                ApplyThemedValue(this, label1.TextColor, nameof(TextColor), Theme.OnPrimaryContainer);
            }

            // Hover State
            if (MouseHover.Value)
            {
                ApplyThemedValue(this, content.Background, nameof(Background), Theme.SecondaryContainer);
                ApplyThemedValue(this, label1.TextColor, nameof(TextColor), Theme.OnSecondaryContainer);
            }
        }

    }
}
