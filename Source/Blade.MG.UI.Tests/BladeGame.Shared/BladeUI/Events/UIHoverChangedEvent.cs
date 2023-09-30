using System;
using System.Collections.Generic;
using System.Text;

namespace BladeGame.Shared.BladeUI.Events
{
    public class UIHoverChangedEvent : UIEvent
    {
        public bool Hover { get; set; }
    }
}
