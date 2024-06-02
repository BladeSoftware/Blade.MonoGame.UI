using Blade.MG.UI.Components;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI
{
    public class TemplatedControl : Control
    {
        [JsonIgnore]
        [XmlIgnore]
        public Type TemplateType { get; set; } // = typeof(ButtonTemplate); // TODO: Validate TemplateType extends UIComponent

        public TemplatedControl()
        {
            // Leave the Hover decision to the Content
            CanHover = false;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            UIComponent templateControl = Activator.CreateInstance(TemplateType) as UIComponent;
            templateControl.HorizontalAlignment = HorizontalAlignmentType.Stretch;
            templateControl.VerticalAlignment = VerticalAlignmentType.Stretch;

            Content = templateControl;
        }

    }
}
