//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Assets.Scripts.BladeUI.Components;

//namespace BladeGame.BladeUI.Controls
//{
//    public class TextBox : BladeControl
//    {
//        public string Text { get; set; }
//        public bool WordWrap { get; set; }
//        public bool MultiLine { get; set; }
//        public int MaxLength { get; set; }

//        public TextBox()
//        {
//            Text = "";
//            WordWrap = false;
//            MultiLine = false;
//            MaxLength = 250;
//        }

//        public override void RenderControl(BladeRect layoutBounds)
//        {
//            base.RenderControl(layoutBounds);

//            GUIStyle textboxStyle = new GUIStyle(GUI.skin.textField);
//            //new GUIStyle { wordWrap = WordWrap, fixedWidth = finalRect.Width, fixedHeight = finalRect.Height, stretchHeight = false, stretchWidth = false, clipping = TextClipping.Clip }

//            if (MultiLine)
//            {
//                Text = GUI.TextArea(finalRect.ToRect(), Text, MaxLength, textboxStyle);
//            }
//            else
//            {
//                Text = GUI.TextField(finalRect.ToRect(), Text, MaxLength, textboxStyle);
//            }

//        }

//    }
//}
