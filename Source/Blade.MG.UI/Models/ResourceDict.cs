using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blade.MG.UI.Models
{
    public class ResourceDict
    {
        public string Name { get; set; }

        private readonly ConcurrentDictionary<string, string> Keys = new ConcurrentDictionary<string, string>();

        public bool TryGetValue(string key, out string value)
        {
            if (Keys == null)
            {
                value = null;
                return false;
            }

            return Keys.TryGetValue(key, out value);
        }

        public void SetValue(string key, string value)
        {
            Keys.AddOrUpdate(key, value, (oldKey, oldValue) => value);
        }

        public void SetValue(string key, Color value)
        {
            SetValue(key, value.ToString());
        }

        public void SetValue(string key, int value)
        {
            SetValue(key, value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        public void SetValue(string key, float value)
        {
            SetValue(key, value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        public void SetValue(string key, Length value)
        {
            SetValue(key, value.ToString());
        }

        public void SetValue(string key, Thickness value)
        {
            SetValue(key, value.ToString());
        }

    }
}
