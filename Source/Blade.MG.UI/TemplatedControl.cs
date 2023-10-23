using Blade.MG.UI.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blade.MG.UI
{
    public class TemplatedControl : Control
    {
        public Type TemplateType { get; set; } // = typeof(ButtonTemplate); // TODO: Validate TemplateType extends UIComponent

        public TemplatedControl()
        {
            // Leave the Hover decision to the Content
            CanHover = false;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            Content = Activator.CreateInstance(TemplateType) as UIComponent;
        }

    }
}
