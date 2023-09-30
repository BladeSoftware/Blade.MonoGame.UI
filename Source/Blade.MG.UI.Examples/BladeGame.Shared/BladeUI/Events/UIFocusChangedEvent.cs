using System;
using System.Collections.Generic;
using System.Text;

namespace BladeGame.Shared.BladeUI.Events
{
    public class UIFocusChangedEvent : UIEvent
    {
        public bool Focused { get; set; }
    }
}
