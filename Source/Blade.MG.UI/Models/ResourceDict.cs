using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blade.MG.UI.Models
{
    public class ResourceDict
    {
        public string Name { get; set; }

        private readonly Dictionary<string, string> Keys = new Dictionary<string, string>();

        public bool TryGetValue(string key, out string value)
        {
            if (Keys == null)
            {
                value = null;
                return false;
            }

            return Keys.TryGetValue(key, out value);
        }

    }
}
