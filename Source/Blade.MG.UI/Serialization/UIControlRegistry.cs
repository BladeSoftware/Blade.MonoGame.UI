using Blade.MG.UI.Controls;

namespace Blade.MG.UI.Serialization
{
    /// <summary>
    /// A closed name -&gt; factory whitelist mapping a UIDocumentNode.Type string to a control
    /// constructor - deliberately NOT a general name-&gt;Type lookup (no Type.GetType(string),
    /// no Activator.CreateInstance(Type) driven by file content). A document can only ever
    /// instantiate a control whose name was registered here by trusted, compiled code before
    /// loading - an unregistered name fails to load rather than resolving to some type found by
    /// searching loaded assemblies. This is the key defense against the classic insecure-
    /// deserialization "arbitrary type instantiation" vulnerability class (the same reason
    /// Newtonsoft's TypeNameHandling.Auto/All is considered dangerous, and why System.Text.Json
    /// doesn't support open-ended polymorphism out of the box).
    ///
    /// Framework controls are pre-registered below. A game project registers its own custom
    /// controls (and, transitively, their own compiled templates - see TemplatedControl) the
    /// same way, via Register&lt;T&gt;, before loading any document that references them by name.
    /// </summary>
    public static class UIControlRegistry
    {
        private static readonly Dictionary<string, Func<UIComponent>> factories = new();
        private static readonly Dictionary<Type, string> namesByType = new();

        static UIControlRegistry()
        {
            Register<Panel>("Panel");
            Register<StackPanel>("StackPanel");
            Register<Grid>("Grid");
            Register<Border>("Border");
            Register<Label>("Label");
            Register<Button>("Button");
            Register<TextBox>("TextBox");
            Register<CheckBox>("CheckBox");
            Register<ComboBox>("ComboBox");
            Register<ScrollPanel>("ScrollPanel");
            Register<IconButton>("IconButton");
            Register<TabPanel>("TabPanel");
            Register<DockPanel>("DockPanel");
        }

        public static void Register<T>(string name) where T : UIComponent, new()
        {
            factories[name] = () => new T();
            namesByType[typeof(T)] = name;
        }

        public static bool IsRegistered(string name) => factories.ContainsKey(name);

        /// <summary>The registered name for a concrete control Type, used by Save to write a
        /// node's Type field - null if the type was never registered (Save refuses to write a
        /// node for a control type nothing can Load back later).</summary>
        public static string GetRegisteredName(Type type) => namesByType.TryGetValue(type, out var name) ? name : null;

        public static UIComponent Create(string name)
        {
            if (string.IsNullOrEmpty(name) || !factories.TryGetValue(name, out var factory))
            {
                throw new UIDocumentException($"Unknown control type '{name}' - it must be registered via UIControlRegistry.Register<T> before it can be loaded.");
            }

            return factory();
        }
    }
}
