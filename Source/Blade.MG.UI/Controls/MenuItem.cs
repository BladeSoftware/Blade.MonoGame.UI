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

        public MenuItem()
        {
            //Text = null;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Top;

            HitTestVisible = true;
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
        public override async Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (!FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                return;
            }

            uiEvent.Handled = true;

            Menu parentMenu = FindParent<Menu>();

            if (parentMenu != null)
            {
                parentMenu.OnMenuItemClicked(this);
            }

            await OnClickAsync?.Invoke(this, uiEvent);

            //base.HandleClickEvent(uiWindow, uiEvent);
        }


    }
}
