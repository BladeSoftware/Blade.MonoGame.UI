using System;

namespace Blade.MG.UI.Controls
{
    /// <summary>
    /// Marks a property as visible/editable in PropertyEditor's reflection-based property grid.
    /// PropertyEditor used to show every public read/write property it found via reflection,
    /// with no way to opt a property out (or a type in) - this attribute replaces that blanket
    /// discovery with an explicit, curated set, and decouples "should this show in the designer"
    /// from "is this a Binding<T>" (which CreateEditor's own isBinding check still uses, but only
    /// to decide how to read/write the value once a property has already passed this filter).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class DesignerPropertyAttribute : Attribute
    {
    }
}
