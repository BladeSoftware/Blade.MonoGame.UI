using System.Reflection;

namespace Blade.MG.UI.Serialization
{
    /// <summary>
    /// Resolves a dotted property path (e.g. "Player.Name") against a runtime-supplied root
    /// object via reflection - the mechanism behind a document's {"$bind": "path"} property
    /// declarations (see UIDocumentSerializer). Deliberately narrow for security: only
    /// BindingFlags.Public | BindingFlags.Instance *properties* are ever read/written - never
    /// non-public or static members, and never a method invocation. A malicious/malformed path
    /// can, at worst, read or write whatever public properties the caller's own chosen root
    /// object (its DataContext) already exposes - it can never reach anything else reachable
    /// from the process (the root object itself is the entire trust boundary, and it's always
    /// the object the host application explicitly chose to hand over, never inferred from the
    /// document).
    /// </summary>
    public static class BindingPath
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;

        public static T Get<T>(object root, string path)
        {
            object owner = ResolveOwner(root, path, out PropertyInfo property);
            if (owner == null || property == null || !property.CanRead)
            {
                return default;
            }

            return property.GetValue(owner) is T typed ? typed : default;
        }

        public static void Set<T>(object root, string path, T value)
        {
            object owner = ResolveOwner(root, path, out PropertyInfo property);
            if (owner == null || property == null || !property.CanWrite)
            {
                return;
            }

            property.SetValue(owner, value);
        }

        /// <summary>Walks every path segment except the last (each must itself be a public
        /// property returning the object to walk into next), then resolves the final segment
        /// as a PropertyInfo on whatever object that walk lands on.</summary>
        private static object ResolveOwner(object root, string path, out PropertyInfo finalProperty)
        {
            finalProperty = null;

            if (root == null || string.IsNullOrEmpty(path))
            {
                return null;
            }

            string[] segments = path.Split('.');
            object current = root;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (current == null)
                {
                    return null;
                }

                PropertyInfo step = current.GetType().GetProperty(segments[i], Flags);
                if (step == null || !step.CanRead)
                {
                    return null;
                }

                current = step.GetValue(current);
            }

            if (current == null)
            {
                return null;
            }

            finalProperty = current.GetType().GetProperty(segments[^1], Flags);
            return finalProperty != null ? current : null;
        }
    }
}
