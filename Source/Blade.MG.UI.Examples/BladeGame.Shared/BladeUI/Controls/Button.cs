using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.BladeUI;
using BladeGame.BladeUI.Components;
using BladeGame.BladeUI.Controls;
using BladeGame.BladeUI.Controls.Templates;
using BladeGame.Shared.BladeUI;
using BladeGame.Shared.BladeUI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.BladeUI.Controls
{
    public class Button : Control
    {
        public Binding<string> Text;
        public Binding<Color> Background;

        public Binding<SpriteFont> SpriteFont { get; set; }
        public Binding<Color> FontColor { get; set; }


        public Button()
        {
            Text = null;
            HorizontalAlignment = HorizontalAlignmentType.Center;
            VerticalAlignment = VerticalAlignmentType.Center;
            HorizontalContentAlignment = HorizontalAlignmentType.Center;
            VerticalContentAlignment =  VerticalAlignmentType.Center;

            HitTestVisible = true;
            IsTabStop = true;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            Content = new ButtonTemplate();
        }

        public override void Measure(UIContext context, Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, availableSize, ref parentMinMax);
        }

        public override void Arrange(Rectangle layoutBounds)
        {
            base.Arrange(layoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds)
        {
            base.RenderControl(context, layoutBounds);
        }


        public Action<UIEvent> OnClick;
        public Action<UIEvent> OnMouseDown;
        public Action<UIEvent> OnMouseUp;
        public Action<UIFocusChangedEvent> OnFocusChanged;
        public Action<UIHoverChangedEvent> OnHoverChanged;


        public override void HandleClickEvent(UIEvent uiEvent)
        {
            base.HandleClickEvent(uiEvent);
            OnClick?.Invoke(uiEvent);
        }

        public override void HandleMouseDownEvent(UIEvent uiEvent)
        {
            base.HandleMouseDownEvent(uiEvent);
            OnMouseDown?.Invoke(uiEvent);
        }

        public override void HandleMouseUpEvent(UIEvent uiEvent)
        {
            base.HandleMouseUpEvent(uiEvent);
            OnMouseUp?.Invoke(uiEvent);
        }

        public override void HandleFocusChangedEvent(UIFocusChangedEvent uiEvent)
        {
            base.HandleFocusChangedEvent(uiEvent);
            OnFocusChanged?.Invoke(uiEvent);
        }

        public override void HandleHoverChanged(UIHoverChangedEvent uiEvent)
        {
            base.HandleHoverChanged(uiEvent);
            OnHoverChanged?.Invoke(uiEvent);
        }


    }
}
