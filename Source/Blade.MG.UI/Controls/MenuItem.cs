using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace Blade.MG.UI.Controls
{
    public class MenuItem : Control
    {
        [JsonIgnore]
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

        // Notifies the parent Menu in addition to the inherited OnActivate firing
        // (UIComponentEvents.ActivateAsync). Overriding this input-agnostic activation handler
        // (rather than relying solely on the OnActivate delegate) means the notification fires
        // consistently for every input type - mouse, keyboard Enter/Space, touch tap, and
        // gamepad A - since they all funnel through ActivateAsync (see UIManager.Keyboard.cs,
        // UIManager.Touch.cs, UIManager.GamePad.cs). No position check needed here - the caller
        // (HandleMouseClickEventAsync's default body, or GamePad/Keyboard calling ActivateAsync
        // directly on the focused component) already establishes that this component is the
        // intended target before ActivateAsync is ever invoked.
        public override async Task ActivateAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            await base.ActivateAsync(uiWindow, uiEvent);

            uiEvent.Handled = true;

            FindParent<Menu>()?.OnMenuItemClicked(this);
        }


    }
}
