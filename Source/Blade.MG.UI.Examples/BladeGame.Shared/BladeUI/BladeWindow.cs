using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BladeGame.BladeUI;
using BladeGame.BladeUI.Components;
using BladeGame.BladeUI.Controls;
using BladeGame.BladeUI.Renderer;
using BladeGame.Shared.BladeUI;
using BladeGame.Shared.BladeUI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BladeGame.BladeUI
{
    public class BladeWindow : Container
    {
        private UIContext context { get; set; }

        public void Init(Game game)
        {
            ContentManager content = new ContentManager(game.Services, "Content");

            context = new UIContext
            {
                Game = game,
                Content = content,
                Pixel = content.Load<Texture2D>("UI/pixel"),
                DefaultFont = content.Load<SpriteFont>("Fonts/Arial"),
                SpriteBatch = new SpriteBatch(game.GraphicsDevice)
            };

            context.Renderer = new UIRenderer(context);
        }

        public void RenderLayout(Rectangle layoutRect)
        {
            Size availableSize = new Size(layoutRect.Width, layoutRect.Height);

            Layout parentMinMax = new Layout(MinWidth, MinHeight, MaxWidth, MaxHeight);

            // Have Children determine their Desired Size
            foreach (var child in Children)
            {
                child.Measure(context, availableSize, ref parentMinMax);
            }

            // Arrange Layout
            foreach (var child in Children)
            {
                child.Arrange(layoutRect);
            }

            // Render Layout
            foreach (var child in Children)
            {
                child.RenderControl(context, layoutRect);
            }

        }


        private KeyboardState keyboardState;
        private KeyboardState lastKeyboardState;
        private MouseState mouseState;
        private MouseState lastMouseState;
        private GamePadState gamePadState;
        private GamePadState lastGamePadState;

        private UIComponent focusedComponent = null;
        private List<UIComponent> hover = new List<UIComponent>();


        public void Logic()
        {
            // Track focused elements and handle keys
            // Converts physical input into 'abstracted' actions


            HandleKeyboardInput();
            HandleMouseInput();
            HandleGamePadInput();


        }

        private void HandleKeyboardInput()
        {
            // Handle Keyboard Input
            lastKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            for (int i = 0; i < pressedKeys.Count(); i++)
            {
                Keys key = pressedKeys[i];

                // If the Key was UP on the last check and now it's down, then the user has just pressed it,
                // so handle the key press
                if (key == Keys.Tab && !lastKeyboardState.IsKeyDown(key))
                {
                    HandleTabNext();
                }
            }
        }

        private void HandleMouseInput()
        {
            // Handle Mouse Input
            lastMouseState = mouseState;
            mouseState = Mouse.GetState();

            // Check if the mouse has moved off of a control is was previously hovering over
            for (int i = hover.Count - 1; i >= 0; i--)
            {
                UIComponent component = hover[i];
                if (component == null || !component.finalRect.Contains(mouseState.Position))
                {
                    hover.RemoveAt(i);
                    RaiseHoverLeaveEvent(component);
                }
            }

            // Check if the mouse is hovering over a control
            //bool selector(UIComponent p) => (p.HitTestVisible && p.finalRect.Contains(mouseState.Position));
            bool selector(UIComponent p) => (p.HitTestVisible && p.finalRect.Contains(mouseState.Position));
            UIComponent selected = SelectFirst(selector);
            if (selected != null)
            {
                if (!hover.Contains(selected))
                {
                    hover.Add(selected);
                    RaiseHoverEnterEvent(selected);
                }
            }

            // Check if user has clicked a mouse button
            if (mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton != ButtonState.Pressed)
            {
                UIComponent component = hover.FirstOrDefault();
                if (component != null)
                {
                    RaiseMouseDownEvent(component);
                    RaiseFocusChangedEvent(component);
                }
            }
            if (mouseState.LeftButton != ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Pressed)
            {
                UIComponent component = hover.FirstOrDefault();
                RaiseMouseUpEvent(component);
                RaiseClickEvent(component);
            }
        }

        private void HandleGamePadInput()
        {
            lastGamePadState = gamePadState;
            gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.IsConnected)
            {
                // ...
            }
        }

        private IEnumerable<UIComponent> SelectAny(Func<UIComponent, bool> selector)
        {
            List<UIComponent> selected = new List<UIComponent>();

            // Local Function to handle recursion
            void SelectComponentsInternal(UIComponent current)
            {
                if (selector(current))
                {
                    selected.Add(current);
                }

                if ((current as Control) != null)
                {
                    if (((Control)current).Content != null)
                    {
                        SelectComponentsInternal(((Control)current).Content);
                    }
                }
                else if ((current as Container) != null)
                {
                    foreach (UIComponent child in ((Container)current).Children)
                    {
                        SelectComponentsInternal(child);
                    }
                }
                else
                {
                }
            }

            // Start Recursive Search
            SelectComponentsInternal(this);

            return selected;
        }

        private UIComponent SelectFirst(Func<UIComponent, bool> selector)
        {
            UIComponent selected = null;

            // Local Function to handle recursion
            void SelectComponentsInternal(UIComponent current)
            {
                //if (current.HitTestVisible)
                //{
                //    if (current.PerformHitTest())
                //    {
                //        selected = current;
                //        return;
                //    }
                //}

                if (selector(current))
                {
                    selected = current;
                    return;
                }


                if ((current as Control) != null)
                {
                    if (((Control)current).Content != null)
                    {
                        SelectComponentsInternal(((Control)current).Content);
                    }
                }
                else if ((current as Container) != null)
                {
                    foreach (UIComponent child in ((Container)current).Children)
                    {
                        SelectComponentsInternal(child);

                        if (selected != null)
                        {
                            return;
                        }
                    }
                }
                else
                {
                }
            }

            // Start Recursive Search
            SelectComponentsInternal(this);

            return selected;
        }

        private void RaiseFocusChangedEvent(UIComponent component)
        {
            // Unfocus previous component
            if (focusedComponent != null && focusedComponent.HasFocus)
            {
                focusedComponent.HasFocus = false;

                Control focusedCtrl1 = focusedComponent as Control;
                if (focusedCtrl1 != null)
                {
                    focusedCtrl1.HandleFocusChangedEvent(new UIFocusChangedEvent { Focused = false });

                }
            }

            // Focus new component
            focusedComponent = component;
            if (focusedComponent != null && !focusedComponent.HasFocus)
            {
                focusedComponent.HasFocus = true;

                Control focusedCtrl = focusedComponent as Control;
                if (focusedCtrl != null)
                {
                    focusedCtrl.HandleFocusChangedEvent(new UIFocusChangedEvent { Focused = true });
                }
            }

        }

        private void RaiseHoverEnterEvent(UIComponent component)
        {
            Control ctrl = component as Control;
            if (ctrl != null)
            {
                ctrl.HandleHoverChanged(new UIHoverChangedEvent { Hover = true });
            }
        }

        private void RaiseHoverLeaveEvent(UIComponent component)
        {
            Control ctrl = component as Control;
            if (ctrl != null)
            {
                ctrl.HandleHoverChanged(new UIHoverChangedEvent { Hover = false });
            }
        }

        private void RaiseMouseDownEvent(UIComponent component)
        {
            Control ctrl = component as Control;
            if (ctrl != null)
            {
                ctrl.HandleMouseDownEvent(new UIEvent { });
            }
        }

        private void RaiseMouseUpEvent(UIComponent component)
        {
            Control ctrl = component as Control;
            if (ctrl != null)
            {
                ctrl.HandleMouseUpEvent(new UIEvent { });
            }
        }

        private void RaiseClickEvent(UIComponent component)
        {
            Control ctrl = component as Control;
            if (ctrl != null)
            {
                ctrl.HandleClickEvent(new UIEvent { });
            }
        }


        private void HandleTabNext()
        {
            int lastTabOrder = -1;

            if (focusedComponent != null)
            {
                lastTabOrder = focusedComponent.TabIndex;
            }

            //Func<UIComponent, bool> selector = (p) => (p.TabIndex > lastTabOrder) && (p.IsTabStop == true);
            bool selector(UIComponent p) => (p.TabIndex > lastTabOrder) && (p.IsTabStop.Value == true);

            IEnumerable<UIComponent> selected = SelectAny(selector);
            if (lastTabOrder > 0 && selected.Count() == 0)
            {
                lastTabOrder = -1;
                selected = SelectAny(selector);
            }

            UIComponent nextControl = selected.FirstOrDefault();
            if (nextControl != null)
            {
                RaiseFocusChangedEvent(nextControl);
            }
        }

        private void HandleSelect()
        {

        }


        //private void HandleTabReverse()
        //{
        //}

    }
}
