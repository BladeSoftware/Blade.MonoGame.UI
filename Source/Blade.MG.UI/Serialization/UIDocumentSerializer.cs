using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Serialization.Converters;

namespace Blade.MG.UI.Serialization
{
    /// <summary>
    /// Save(UIComponent) / Load(UIDocument) - converts between the live UIComponent tree and
    /// the portable UIDocument DTO tree (see UIDocument.cs). Deliberately does not hand the live
    /// UIComponent/Control/Container classes straight to System.Text.Json's own reflection-based
    /// (de)serialization - instead walks each node by hand via a small, explicit set of rules,
    /// so control-tree structure (Content/Children/GridPlacement), the closed control-type
    /// whitelist (UIControlRegistry), and the literal-vs-$bind property distinction all stay
    /// under this class's direct control rather than System.Text.Json's own polymorphism rules.
    /// </summary>
    public static class UIDocumentSerializer
    {
        private const string BindMarker = "$bind";

        // A generous but explicit ceiling on the raw JSON text handed to the parser - guards
        // against a maliciously huge design file, independent of MaxDepth below (see the
        // design plan's Security section).
        public const int MaxDocumentSizeBytes = 10 * 1024 * 1024; // 10 MB

        private static readonly JsonSerializerOptions jsonOptions = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                // Explicit rather than relying on the library default, so a maliciously deep/
                // nested property value can't cause runaway recursion - the control *tree*
                // itself is walked by hand (LoadNode/BuildNode), not by this option, but nested
                // property values (e.g. RowDefinition lists) still go through JsonSerializer.
                MaxDepth = 64,
            };

            options.Converters.Add(new JsonColorConverter());
            options.Converters.Add(new JsonThicknessConverter());
            options.Converters.Add(new JsonCornerRadiusConverter());
            options.Converters.Add(new JsonGridLengthConverter());
            options.Converters.Add(new JsonVector2Converter());
            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        }

        // ---=== Save ===---

        public static UIDocument Save(UIComponent root)
        {
            return new UIDocument { Root = BuildNode(root) };
        }

        public static string SaveToJson(UIComponent root)
        {
            return JsonSerializer.Serialize(Save(root), jsonOptions);
        }

        private static UIDocumentNode BuildNode(UIComponent component)
        {
            if (component == null)
            {
                return null;
            }

            string typeName = UIControlRegistry.GetRegisteredName(component.GetType())
                ?? throw new UIDocumentException($"Cannot save a '{component.GetType().Name}' - it isn't registered in UIControlRegistry.");

            var node = new UIDocumentNode
            {
                Type = typeName,
                Name = component.Name,
            };

            foreach (MemberInfo member in GetSerializableMembers(component.GetType()))
            {
                object rawValue = GetMemberValue(member, component);
                if (rawValue == null)
                {
                    continue; // omit - Load leaves the freshly-constructed default in place
                }

                Type valueType = UnwrapBindingType(GetMemberType(member), out bool isBinding);

                object literalValue = rawValue;
                if (isBinding)
                {
                    literalValue = GetBindingValue(rawValue);
                    if (literalValue == null)
                    {
                        continue;
                    }
                }

                if (literalValue is Length length && float.IsNaN(length.Value))
                {
                    // NaN is Length's own "not set" sentinel (see FloatHelper.IsNaN/ValueOrZero) -
                    // omit rather than round-trip it, since Length.FromString doesn't accept the
                    // "NaN <unit>" text its own ToString()/JsonLengthConverter.Write can produce
                    // for an unset dimension (e.g. a Width never explicitly assigned).
                    continue;
                }

                node.Properties[member.Name] = JsonSerializer.SerializeToElement(literalValue, valueType, jsonOptions);
            }

            if (component is Grid grid)
            {
                foreach (UIComponent child in grid.Children)
                {
                    UIDocumentNode childNode = BuildNode(child);
                    childNode.GridPlacement = new GridPlacement
                    {
                        Column = grid.GetColumn(child),
                        Row = grid.GetRow(child),
                        ColumnSpan = grid.GetColumnSpan(child),
                        RowSpan = grid.GetRowSpan(child),
                    };
                    (node.Children ??= new List<UIDocumentNode>()).Add(childNode);
                }
            }
            // TabPanel.AddChild throws (tab content lives in a private list, not the base
            // Children collection - see TabPanel.TabPages) so it needs its own walk rather than
            // falling into the generic Container branch below.
            else if (component is TabPanel tabPanel)
            {
                foreach (var (tabContent, header) in tabPanel.TabPages)
                {
                    UIDocumentNode childNode = BuildNode(tabContent);
                    childNode.TabHeader = header;
                    (node.Children ??= new List<UIDocumentNode>()).Add(childNode);
                }
            }
            // DockPanel's own Children mixes its 5 region Panels with internal SplitterBar
            // instances (SplitterBar isn't registrable) - walk the named regions directly instead.
            else if (component is DockPanel dockPanel)
            {
                AddRegionIfPresent(node, "Left", dockPanel.LeftPanel);
                AddRegionIfPresent(node, "Right", dockPanel.RightPanel);
                AddRegionIfPresent(node, "Top", dockPanel.TopPanel);
                AddRegionIfPresent(node, "Bottom", dockPanel.BottomPanel);
                AddRegionIfPresent(node, "Center", dockPanel.CenterPanel);
            }
            else if (component is Container container)
            {
                foreach (UIComponent child in container.Children)
                {
                    (node.Children ??= new List<UIDocumentNode>()).Add(BuildNode(child));
                }
            }
            // TemplatedControl subclasses (CheckBox, TextBox, Button, ComboBox, ...) can have a
            // non-null Content too - but it's their own framework-managed template instance
            // (e.g. ComboBox.InitTemplate assigns Content directly rather than using
            // AddInternalChild like the TemplatedControl base does), not a designer-authored
            // child, and isn't registered in UIControlRegistry - so it must never be walked
            // here. Only a plain Control (e.g. Border) has a genuine user-facing Content.
            else if (component is Control control && component is not TemplatedControl && control.Content != null)
            {
                node.Content = BuildNode(control.Content);
            }

            return node;
        }

        // A DockPanel region (LeftPanel/RightPanel/TopPanel/BottomPanel/CenterPanel) holds at
        // most one designer-authored child in normal usage (see DockPanel usages elsewhere in
        // the codebase) - only its first child is saved, matching how Load puts a loaded region
        // node's content back via a single AddChild.
        private static void AddRegionIfPresent(UIDocumentNode node, string regionName, Panel regionPanel)
        {
            UIComponent regionContent = regionPanel.Children.FirstOrDefault();
            if (regionContent == null)
            {
                return;
            }

            (node.Regions ??= new Dictionary<string, UIDocumentNode>())[regionName] = BuildNode(regionContent);
        }

        // ---=== Load ===---

        /// <param name="dataContext">The object $bind property paths are resolved against -
        /// the host application's own data (e.g. a SettingsViewModel), never inferred from the
        /// document itself. Pass null if the document has no $bind properties.</param>
        public static UIComponent Load(UIDocument document, object dataContext = null)
        {
            if (document?.Root == null)
            {
                throw new UIDocumentException("Document has no Root node.");
            }

            return LoadNode(document.Root, dataContext);
        }

        public static UIComponent LoadFromJson(string json, object dataContext = null)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new UIDocumentException("Document JSON is empty.");
            }

            if (json.Length > MaxDocumentSizeBytes)
            {
                throw new UIDocumentException($"Document exceeds the maximum allowed size ({MaxDocumentSizeBytes} bytes).");
            }

            UIDocument document = JsonSerializer.Deserialize<UIDocument>(json, jsonOptions);
            return Load(document, dataContext);
        }

        private static UIComponent LoadNode(UIDocumentNode node, object dataContext)
        {
            UIComponent instance = UIControlRegistry.Create(node.Type);
            instance.Name = node.Name;

            foreach (var (propertyName, element) in node.Properties)
            {
                MemberInfo member = FindMember(instance.GetType(), propertyName);
                if (member == null)
                {
                    continue; // unknown/renamed property - ignore rather than fail the whole load
                }

                AssignMember(instance, member, element, dataContext);
            }

            if (node.Content != null)
            {
                if (instance is not Control control)
                {
                    throw new UIDocumentException($"'{node.Type}' has a Content node in the document but isn't a Control.");
                }

                control.Content = LoadNode(node.Content, dataContext);
            }

            if (node.Children != null)
            {
                if (instance is Grid grid)
                {
                    foreach (UIDocumentNode childNode in node.Children)
                    {
                        UIComponent child = LoadNode(childNode, dataContext);
                        GridPlacement placement = childNode.GridPlacement ?? new GridPlacement();
                        grid.AddChild(child, placement.Column, placement.ColumnSpan, placement.Row, placement.RowSpan);
                    }
                }
                else if (instance is TabPanel tabPanel)
                {
                    foreach (UIDocumentNode childNode in node.Children)
                    {
                        UIComponent child = LoadNode(childNode, dataContext);
                        tabPanel.AddTab(child, childNode.TabHeader ?? "");
                    }
                }
                else if (instance is Container container)
                {
                    foreach (UIDocumentNode childNode in node.Children)
                    {
                        container.AddChild(LoadNode(childNode, dataContext));
                    }
                }
                else
                {
                    throw new UIDocumentException($"'{node.Type}' has Children in the document but isn't a Container.");
                }
            }

            if (node.Regions != null)
            {
                if (instance is not DockPanel dockPanel)
                {
                    throw new UIDocumentException($"'{node.Type}' has Regions in the document but isn't a DockPanel.");
                }

                AssignRegion(dockPanel.LeftPanel, node.Regions, "Left", dataContext);
                AssignRegion(dockPanel.RightPanel, node.Regions, "Right", dataContext);
                AssignRegion(dockPanel.TopPanel, node.Regions, "Top", dataContext);
                AssignRegion(dockPanel.BottomPanel, node.Regions, "Bottom", dataContext);
                AssignRegion(dockPanel.CenterPanel, node.Regions, "Center", dataContext);
            }

            return instance;
        }

        private static void AssignRegion(Panel regionPanel, Dictionary<string, UIDocumentNode> regions, string regionName, object dataContext)
        {
            if (regions.TryGetValue(regionName, out UIDocumentNode regionNode))
            {
                regionPanel.AddChild(LoadNode(regionNode, dataContext));
            }
        }

        private static void AssignMember(UIComponent instance, MemberInfo member, JsonElement element, object dataContext)
        {
            Type valueType = UnwrapBindingType(GetMemberType(member), out bool isBinding);

            object valueToAssign;

            if (isBinding && element.ValueKind == JsonValueKind.Object && element.TryGetProperty(BindMarker, out JsonElement bindPathElement))
            {
                string path = bindPathElement.GetString();
                if (string.IsNullOrEmpty(path) || dataContext == null)
                {
                    return; // nothing to bind against - leave the constructor's own default in place
                }

                valueToAssign = CreateBoundBinding(valueType, dataContext, path);
            }
            else
            {
                object literal = element.Deserialize(valueType, jsonOptions);
                valueToAssign = isBinding ? CreateValueBinding(valueType, literal) : literal;
            }

            SetMemberValue(member, instance, valueToAssign);
        }

        // ---=== Reflection helpers ===---

        // Only members meant to be plain data - skips [JsonIgnore]-marked infrastructure,
        // Content/Children/Name (walked/assigned separately), Type/Delegate-valued members
        // (template types and event handlers must never appear in the document - see the
        // design plan's Security section), and any UIComponent-typed/collection-of-UIComponent
        // property (also walked separately, as Content/Children).
        private static IEnumerable<MemberInfo> GetSerializableMembers(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                if (property.GetGetMethod() != null && IsSerializableMember(property, property.PropertyType))
                {
                    yield return property;
                }
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsSerializableMember(field, field.FieldType))
                {
                    yield return field;
                }
            }
        }

        private static bool IsSerializableMember(MemberInfo member, Type memberType)
        {
            if (member.Name is "Content" or "Children" or "Name")
            {
                return false;
            }

            if (Attribute.IsDefined(member, typeof(JsonIgnoreAttribute)))
            {
                return false;
            }

            if (typeof(UIComponent).IsAssignableFrom(memberType))
            {
                return false;
            }

            if (typeof(Delegate).IsAssignableFrom(memberType))
            {
                return false; // event handlers/callbacks are behavior, never document data
            }

            if (typeof(Type).IsAssignableFrom(memberType))
            {
                return false; // TemplateType/ItemTemplateType-style members - never named in the document
            }

            if (IsUIComponentCollection(memberType))
            {
                return false;
            }

            return true;
        }

        private static bool IsUIComponentCollection(Type memberType)
        {
            if (!memberType.IsGenericType || !typeof(IEnumerable).IsAssignableFrom(memberType))
            {
                return false;
            }

            Type[] genericArgs = memberType.GetGenericArguments();
            return genericArgs.Length == 1 && typeof(UIComponent).IsAssignableFrom(genericArgs[0]);
        }

        private static MemberInfo FindMember(Type type, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                return property;
            }

            return type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
        }

        private static Type GetMemberType(MemberInfo member) => member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new UIDocumentException($"Unsupported member kind for '{member.Name}'."),
        };

        private static object GetMemberValue(MemberInfo member, object instance) => member switch
        {
            PropertyInfo property => property.GetValue(instance),
            FieldInfo field => field.GetValue(instance),
            _ => null,
        };

        private static void SetMemberValue(MemberInfo member, object instance, object value)
        {
            switch (member)
            {
                case PropertyInfo property when property.GetSetMethod() != null:
                    property.SetValue(instance, value);
                    break;

                case FieldInfo field:
                    field.SetValue(instance, value);
                    break;
            }
        }

        private static Type UnwrapBindingType(Type memberType, out bool isBinding)
        {
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Binding<>))
            {
                isBinding = true;
                return memberType.GetGenericArguments()[0];
            }

            isBinding = false;
            return memberType;
        }

        private static object GetBindingValue(object bindingInstance)
        {
            PropertyInfo valueProperty = bindingInstance.GetType().GetProperty("Value");
            return valueProperty.GetValue(bindingInstance);
        }

        private static object CreateValueBinding(Type valueType, object literalValue)
        {
            Type bindingType = typeof(Binding<>).MakeGenericType(valueType);
            return Activator.CreateInstance(bindingType, literalValue, false);
        }

        private static object CreateBoundBinding(Type valueType, object dataContext, string path)
        {
            Type factoryType = typeof(BoundPropertyFactory<>).MakeGenericType(valueType);
            object factory = Activator.CreateInstance(factoryType, dataContext, path);
            return factoryType.GetMethod(nameof(BoundPropertyFactory<object>.Create)).Invoke(factory, null);
        }

        // Bridges a runtime-only Type (valueType) to a strongly-typed Binding<T> whose getter/
        // setter reflect against dataContext via BindingPath - see the $bind design notes.
        // Written once, generically; Activator.CreateInstance closes it over the actual T at
        // load time, the standard technique for invoking a generic operation when T is only
        // known as a System.Type at runtime.
        private sealed class BoundPropertyFactory<T>
        {
            private readonly object dataContext;
            private readonly string path;

            public BoundPropertyFactory(object dataContext, string path)
            {
                this.dataContext = dataContext;
                this.path = path;
            }

            public Binding<T> Create()
            {
                return new Binding<T>(
                    () => BindingPath.Get<T>(dataContext, path),
                    v => BindingPath.Set(dataContext, path, v));
            }
        }
    }
}
