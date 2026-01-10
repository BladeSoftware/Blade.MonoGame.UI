using Blade.MG.UI.Components;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI
{
    public class TemplatedControl : Control
    {
        [JsonIgnore]
        [XmlIgnore]
        public Type TemplateType
        {
            get => templateType;

            set
            {
                // Validate TemplateType extends UIComponent
                if (value.IsSubclassOf(typeof(UIComponent)))
                {
                    templateType = value;
                }
                else
                {
                    throw new System.Exception("TemplateType must extend UIComponent");
                }
            }
        }
        private Type templateType;


        // Leave the Hover / Focus decision to the main Control
        //public new bool CanHover { get => Parent.CanHover; set => Parent.CanHover = value; }
        //public new bool CanFocus { get => Parent.CanFocus; set => Parent.CanFocus = value; }


        protected override void InitTemplate()
        {
            base.InitTemplate();

            //// Leave Hover and Focus to the main Control
            //CanHover = false;
            //CanFocus = false;

            UIComponent templateControl = Activator.CreateInstance(TemplateType) as UIComponent;
            templateControl.HorizontalAlignment = HorizontalAlignmentType.Stretch;
            templateControl.VerticalAlignment = VerticalAlignmentType.Stretch;

            // Leave Hover and Focus to the main Control
            templateControl.CanHover = false;
            templateControl.CanFocus = false;

            AddInternalChild(templateControl);
        }

    }
}
