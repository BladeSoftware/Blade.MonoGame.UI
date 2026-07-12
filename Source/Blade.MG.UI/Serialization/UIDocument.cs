using System.Text.Json;

namespace Blade.MG.UI.Serialization
{
    /// <summary>
    /// The root of a saved control/window design - a portable, data-only description of a
    /// UIComponent tree (see UIDocumentNode), never the live UIComponent/Control/Container
    /// classes themselves. SchemaVersion exists so a future format change can be detected and
    /// migrated rather than silently misinterpreted.
    /// </summary>
    public class UIDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public UIDocumentNode Root { get; set; }
    }

    /// <summary>
    /// One node in a saved control tree.
    ///
    /// Type is a name registered via UIControlRegistry - never an assembly-qualified type name
    /// or anything else that could resolve to an arbitrary loaded type (see UIControlRegistry
    /// and UIDocumentSerializer for why - this is the key security boundary of the whole format).
    ///
    /// Properties holds literal values or {"$bind": "some.path"} binding declarations - see
    /// UIDocumentSerializer for how each is resolved (a literal becomes a plain-value Binding&lt;T&gt;
    /// or plain property assignment; a $bind declaration becomes a live, reflection-backed
    /// Binding&lt;T&gt; against the DataContext object supplied to Load).
    ///
    /// Content is used when this node's control is a Control (single child, e.g. Border).
    /// Children is used when this node's control is a Container (Panel/StackPanel/Grid/...).
    /// A node should populate at most one of the two, matching whatever the live control
    /// actually is - the loader does not attempt to reconcile both being set.
    ///
    /// GridPlacement is only meaningful for a node listed in a parent Grid node's Children -
    /// Grid places children via SetColumn/SetRow/etc. (an attached-property-style mechanism
    /// keyed by child instance, not a property on the child itself), so it needs its own slot
    /// here rather than living on the child's own Properties.
    /// </summary>
    public class UIDocumentNode
    {
        public string Type { get; set; }
        public string Name { get; set; }

        public Dictionary<string, JsonElement> Properties { get; set; } = new();

        public UIDocumentNode Content { get; set; }
        public List<UIDocumentNode> Children { get; set; }

        public GridPlacement GridPlacement { get; set; }

        /// <summary>Only meaningful for a node listed in a parent TabPanel node's Children -
        /// TabPanel content isn't addressable through plain Children/AddChild (see
        /// UIDocumentSerializer's TabPanel handling), so each tab's header text (TabPanel shows
        /// it via DataContext.ToString() - see TabHeaderTemplate) travels alongside its content
        /// node the same way GridPlacement does for a Grid child.</summary>
        public string TabHeader { get; set; }

        /// <summary>Only meaningful on a DockPanel node - DockPanel has 5 named single-slot
        /// regions (Left/Right/Top/Bottom/Center), not a Children list (its own Children/AddChild
        /// mixes those regions with internal splitter bars - see UIDocumentSerializer's DockPanel
        /// handling), so each region's single child (if any) is keyed by region name here instead.</summary>
        public Dictionary<string, UIDocumentNode> Regions { get; set; }
    }

    public class GridPlacement
    {
        public int Column { get; set; }
        public int Row { get; set; }
        public int ColumnSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
    }

    /// <summary>Thrown for any failure in saving/loading a UIDocument - e.g. an unregistered
    /// control Type name, or a $bind path that doesn't resolve against the supplied DataContext.</summary>
    public class UIDocumentException : Exception
    {
        public UIDocumentException(string message) : base(message) { }
        public UIDocumentException(string message, Exception innerException) : base(message, innerException) { }
    }
}
