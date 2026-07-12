namespace Blade.MG.UI.Serialization
{
    /// <summary>
    /// Post-load lookup: after UIDocumentSerializer.Load returns a tree, the consuming game
    /// finds specific controls by the Name the designer gave them, to attach its own event
    /// handlers/business logic in code - e.g.
    /// ((Button)root.FindByName("SaveButton")).OnActivate = ...
    /// A plain recursive walk over both shapes a UIComponent can take (Control.Content,
    /// Container.Children) - not a property on UIComponent itself, since Content/Children are
    /// only ever added by those two subclasses.
    /// </summary>
    public static class UIComponentTreeExtensions
    {
        public static UIComponent FindByName(this UIComponent root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (root.Name == name)
            {
                return root;
            }

            if (root is Control control && control.Content != null)
            {
                UIComponent found = control.Content.FindByName(name);
                if (found != null)
                {
                    return found;
                }
            }

            if (root is Container container)
            {
                foreach (UIComponent child in container.Children)
                {
                    UIComponent found = child.FindByName(name);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }
    }
}
