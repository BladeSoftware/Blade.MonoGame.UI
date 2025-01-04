using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;

namespace Blade.MG.UI.Models
{
    public class ResourceDict
    {
        public string Name { get; set; }

        // A dictionary indexed by Resource Name (e.g. "TEXTBOX"). Resource Name defaults to Blank
        // The Resulting dictionary is a Key-Value pair of settings: e.g. "BackgroundColour" -> "#FF0000"
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> ResourceDicts;

        public bool TryGetValue(string key, out string value)
        {
            return TryGetValue("", key, out value);
        }

        public bool TryGetValue(string resource, string key, out string value)
        {
            if (ResourceDicts == null)
            {
                value = null;
                return false;
            }

            // Try fetch a resource dictionary with the given name
            if (!ResourceDicts.TryGetValue(resource ?? "", out var dict))
            {
                value = null;
                return false;
            }


            // Try lookup a Key-Value setting from the resource dictionary
            return dict.TryGetValue(key, out value);
        }


        public void SetValue(string key, object value)
        {
            if (value == null)
            {
                throw new ArgumentException("Value cannot be Null");
            }

            SetValue(key, value.ToString());
        }

        public void SetValue(string resource, string key, object value)
        {
            if (value == null)
            {
                throw new ArgumentException("Value cannot be Null");
            }

            SetValue(resource, key, value.ToString());
        }

        public void SetValue(string key, string value)
        {
            SetValue("", key, value);
        }

        public void SetValue(string resource, string key, string value)
        {
            if (ResourceDicts == null)
            {
                lock (this)
                {
                    if (ResourceDicts == null)
                    {
                        ResourceDicts = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            // Try fetch a resource dictionary with the given name
            if (!ResourceDicts.TryGetValue(resource ?? "", out var dict))
            {
                // There's no existing resource dictionary with this name, try add one
                lock (this)
                {
                    // Make sure another thread hasn't already added it
                    if (!ResourceDicts.TryGetValue(resource ?? "", out dict))
                    {
                        // Still doesn't exist, so create it
                        dict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        if (!ResourceDicts.TryAdd(resource ?? "", dict))
                        {
                            // Add failed, Someone else already added a resource dictionary with the same name, so fetch the existing one
                            if (!ResourceDicts.TryGetValue(resource ?? "", out dict))
                            {
                                // We still didn't find a resource dictionary
                                return;
                            }
                        }
                    }
                }
            }

            dict.AddOrUpdate(key, value, (oldKey, oldValue) => value);
        }


        //public void SetValue(string key, string value)
        //{
        //    ResourceDicts.AddOrUpdate(key, value, (oldKey, oldValue) => value);
        //}

        //public void SetValue(string key, Color value)
        //{
        //    SetValue(key, value.ToString());
        //}

        //public void SetValue(string key, int value)
        //{
        //    SetValue(key, value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        //}

        //public void SetValue(string key, float value)
        //{
        //    SetValue(key, value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        //}

        //public void SetValue(string key, Length value)
        //{
        //    SetValue(key, value.ToString());
        //}

        //public void SetValue(string key, Thickness value)
        //{
        //    SetValue(key, value.ToString());
        //}


        //public T GetResourceValue<T>(string property)
        //{
        //    return GetResourceValue<T>(ResourceKey, property);
        //}

        public static T GetResourceValue<T>(string resource, Style<T> style)
        {
            foreach (var part in style.Parts)
            {
                if (TryGetResourceValue<T>(resource, part, out T value))
                {
                    return value;
                }
            }

            return default(T);
        }

        public static bool TryGetResourceValue<T>(string resource, string property, out T value)
        {
            //string value = GetResourceValue(resource, property);
            value = default(T);

            if (!TryGetResourceValue(resource, property, out string stringValue))
            {
                return false;
            }

            Type typeT = typeof(T);

            if (typeT == typeof(Color))
            {
                var color = ((UIColor)Activator.CreateInstance(typeof(UIColor), stringValue)).ToColor();
                value = (T)Activator.CreateInstance(typeof(Color), color.R, color.G, color.B, color.A);
                return true;
            }

            if (typeT == typeof(UIColor) || typeT == typeof(Length) || typeT == typeof(Thickness))
            {
                value = (T)Activator.CreateInstance(typeT, stringValue);
                return true;
            }

            try
            {
                value = (T)Convert.ChangeType(stringValue, typeT);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

            //if (typeT.IsEquivalentTo(typeof(string)))
            //{
            //    return (T)Convert.ChangeType("ABC", typeof(string));
            //}

            //if (typeT.IsEquivalentTo(typeof(float)))
            //{
            //    return (T)Convert.ChangeType("123.45", typeof(float));
            //}

            //return default(T);
        }

        public static T GetResourceValue<T>(string resource, string property)
        {
            string value = GetResourceValue(resource, property);

            Type typeT = typeof(T);

            if (typeT == typeof(Color))
            {
                var color = ((UIColor)Activator.CreateInstance(typeof(UIColor), value)).ToColor();
                return (T)Activator.CreateInstance(typeof(Color), color.R, color.G, color.B, color.A);
            }

            if (typeT == typeof(UIColor) || typeT == typeof(Length) || typeT == typeof(Thickness))
            {
                return (T)Activator.CreateInstance(typeT, value);
            }

            try
            {
                return (T)Convert.ChangeType(value, typeT);
            }
            catch (Exception ex)
            {
                return default(T);
            }

            //if (typeT.IsEquivalentTo(typeof(string)))
            //{
            //    return (T)Convert.ChangeType("ABC", typeof(string));
            //}

            //if (typeT.IsEquivalentTo(typeof(float)))
            //{
            //    return (T)Convert.ChangeType("123.45", typeof(float));
            //}

            //return default(T);
        }


        //public string GetResourceValue(string property)
        //{
        //    return GetResourceValue(ResourceKey, property);
        //}

        public static string GetResourceValue(string resource, string property)
        {
            TryGetResourceValue(resource, property, out var value);

            return value;
        }

        public static bool TryGetResourceValue(string resource, string property, out string value)
        {
            value = string.Empty;

            //// First try the local window resource dictionary
            //var windowResourceDict = ParentWindow?.ResourceDict;
            //if (windowResourceDict != null)
            //{
            //    if (windowResourceDict.TryGetValue(resource, property, out value))
            //    {
            //        return value;
            //    }

            //    if (windowResourceDict.TryGetValue(property, out value))
            //    {
            //        return value;
            //    }
            //}

            // Then the global resource dictionary
            if (UIManager.ResourceDict != null)
            {
                if (UIManager.ResourceDict.TryGetValue(resource, property, out value))
                {
                    return true;
                }

                if (UIManager.ResourceDict.TryGetValue(property, out value))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
