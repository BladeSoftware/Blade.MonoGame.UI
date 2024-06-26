﻿using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI.Controls
{
    public class TextBox : TemplatedControl
    {
        private Binding<string> text;

        public Binding<string> Text
        {
            get { return text; }
            set
            {
                text.Value = value.Value == null || value.Value.Length <= MaxLength ? value.Value : value.Value.Substring(0, MaxLength);
            }
        }

        public Binding<HorizontalAlignmentType> HorizontalTextAlignment;
        public Binding<VerticalAlignmentType> VerticalTextAlignment;

        private Binding<Color> textColor = new Binding<Color>();
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        public Binding<string> FontName { get; set; } = new Binding<string>();

        public Binding<float> FontSize { get; set; } = new Binding<float>();

        public bool WordWrap { get; set; }

        public bool MultiLine { get; set; }

        public int MaxLength { get; set; }

        //private int cursorTextIndex;

        public int CursorPosition { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }

        public Variant Variant { get; set; }
        public string Label { get; set; }
        public string HelperText { get; set; }
        public bool Underline { get; set; } // Underline the text
        public bool ShrinkLabel { get; set; } // Label stays Shrunk and doesn't fill the textbox if the text is empty


        public TextBox()
        {
            TemplateType = typeof(TextBoxTemplate);

            HorizontalTextAlignment = HorizontalAlignmentType.Left;
            VerticalTextAlignment = VerticalAlignmentType.Bottom;

            TextColor = Color.Black;

            IsTabStop = true;
            IsHitTestVisible = true;

            FontName = null; // Use default
            FontSize = null; // Use default

            text = "";

            WordWrap = false;
            MultiLine = false;
            MaxLength = 250;

            CursorPosition = 0;
            SelectionStart = 0;
            SelectionLength = 0;

            Variant = Variant.Standard;
            Label = null;
            HelperText = null;

            //MinHeight = 30;

            Underline = true;
            ShrinkLabel = false;

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            Content = new TextBoxTemplate();
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            MergeChildDesiredSize(context, ref availableSize, Content, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            if (Content != null)
            {
                Content.Arrange(context, FinalContentRect, FinalContentRect);
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, layoutBounds, parentTransform);
        }

        // ---=== UI Events ===---

        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            await base.HandleKeyPressAsync(uiWindow, uiEvent);

            if (!uiEvent.Handled && HasFocus.Value)
            {
                HandleKey(uiEvent);
            }
        }

        private void HandleKey(UIKeyEvent uiEvent)
        {
            if (uiEvent.KeyChar != null)
            {
                AddChar(uiEvent.KeyChar);
                uiEvent.Handled = true;
            }
            else
            {
                // Handle Special Keys
                switch (uiEvent.Key)
                {
                    case Keys.Back:
                        HandleBackspace();
                        uiEvent.Handled = true;
                        break;

                    case Keys.Delete:
                        HandleDelete();
                        uiEvent.Handled = true;
                        break;
                }
            }

        }

        private void AddChar(string character)
        {
            if (Text.Value.Length < MaxLength)
            {
                Text.Value += character;
            }
        }

        private void HandleBackspace()
        {
            if (Text.Value.Length > 0)
            {
                Text.Value = Text.Value.Substring(0, Text.Value.Length - 1);
            }
        }

        private void HandleDelete()
        {
            if (Text.Value.Length > 0)
            {
                //Text.Value = Text.Value.Substring(0, Text.Value.Length - 1);
            }
        }


        //public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        //{
        //    //return base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
        //    this.HasFocus = uiEvent.Focused;
        //}

        //protected override void HandleStateChange()
        //{
        //    base.HandleStateChange();
        //}

    }
}
