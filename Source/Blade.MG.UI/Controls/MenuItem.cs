using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Controls
{
    public class MenuItem : Control
    {
        [JsonIgnore]
        [XmlIgnore]
        public Type MenuItemTemplate { get; set; }

        private Binding<Color> textColor = new Binding<Color>();
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        public MenuItem()
        {
            //Text = null;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Top;

            IsHitTestVisible = true;
            IsTabStop = true;
        }


        protected override void InitTemplate()
        {
            base.InitTemplate();

            UIComponent itemTemplate = Activator.CreateInstance(MenuItemTemplate ?? typeof(MenuItemTemplate)) as UIComponent;
            if (itemTemplate == null)
            {
                throw new Exception("Menu Item Template must derive from UIComponent");
            }

            itemTemplate.DataContext = DataContext;
            Content = itemTemplate;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);
        }

        // ---=== UI Events ===---

        // Notifies the parent Menu in addition to the inherited OnMouseClick/OnPrimaryClick
        // firing (UIComponentEvents.HandleMouseClickEventAsync). Overriding this method
        // instead of HandlePrimaryClickEventAsync means the notification fires consistently
        // for every input type - mouse, keyboard Enter/Space, touch tap, and gamepad A - since
        // they all now dispatch through HandleMouseClickEventAsync (see UIManager.Keyboard.cs,
        // UIManager.Touch.cs, UIManager.GamePad.cs).
        public override async Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            await base.HandleMouseClickEventAsync(uiWindow, uiEvent);

            if (!ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)))
            {
                return;
            }

            uiEvent.Handled = true;

            FindParent<Menu>()?.OnMenuItemClicked(this);
        }


    }
}
