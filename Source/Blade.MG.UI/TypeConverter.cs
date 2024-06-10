using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blade.MG.UI
{
    internal class TypeConverter
    {
        //public void UpdateProperty(string propertyName, string strValue)
        //{
        //    UpdateProperty(this, propertyName, strValue);
        //}

        public static void UpdateProperty(object instance, string propertyName, string strValue)
        {
            if (string.IsNullOrEmpty(propertyName)) { return; }

            var prop = instance.GetType().GetProperty(propertyName);

            if (prop == null) { throw new ArgumentException($"Property {propertyName} not found on Type {instance.GetType().FullName}"); }

            Type propertyType = prop.PropertyType;
            string propertyTypeName = propertyType.Name;

            Type propGenericType = null;
            string propGenericTypeName = null;

            Type rawType = propertyType;
            string rawTypeName = propertyTypeName;

            if (propertyType.IsGenericType)
            {
                propGenericType = prop.PropertyType.GenericTypeArguments.FirstOrDefault();
                propGenericTypeName = prop.PropertyType.GenericTypeArguments.FirstOrDefault()?.Name;

                rawType = propGenericType;
                rawTypeName = propGenericTypeName;
            }

            bool isBindingType = propertyTypeName == "Binding`1";


            if (propertyTypeName == "String")
            {
                // Assign String to String
                prop.SetValue(instance, strValue, null);
                return;
            }
            else if (isBindingType && string.Equals(propGenericTypeName, "String"))
            {
                // We are assigning a string to a property of Type Binding<String>
                prop.SetValue(instance, new Binding<string>(strValue), null);
                return;
            }


            // Create IParsable<T>
            bool implementsIParsable = rawType.GetInterface("IParsable`1") != null;

            object parsedValue = null;
            bool isConverted = false;

            if (implementsIParsable)
            {
                //Type parsableType = typeof(IParsable<>).MakeGenericType(propertyType);
                //if (propertyType.IsAssignableTo(parsableType)) ...

                var parseMethod = rawType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(string), typeof(CultureInfo) });
                if (parseMethod == null)
                {
                    throw new Exception($"Property {rawTypeName} is not castable to type IParsable<>");
                }

                //var x = Int32.Parse("123", System.Globalization.CultureInfo.InvariantCulture);
                parsedValue = parseMethod.Invoke(instance, new object[] { strValue, CultureInfo.InvariantCulture });
                isConverted = true;
            }

            // Need a way to handle special cases e.g. Register list of converters for specific types
            if (!isConverted)
            {
                if (rawType.FullName == "Microsoft.Xna.Framework.Color")
                {
                    parsedValue = ColorHelper.FromString(strValue);
                    isConverted = true;
                }
            }


            if (isConverted)
            {
                if (isBindingType)
                {
                    // Convert to Binding<T> e.g. Binding<int32> = int32
                    Type bindingGenericType = typeof(Binding<>).MakeGenericType(propGenericType);
                    var bindingInstance = Activator.CreateInstance(bindingGenericType, parsedValue);

                    prop.SetValue(instance, bindingInstance, null);
                }
                else if (propertyType.IsGenericType)
                {
                    var bindingInstance = Activator.CreateInstance(propertyType, parsedValue);

                    prop.SetValue(instance, bindingInstance, null);
                }
                else
                {
                    // Direct Assignment. e.g. in32 = int32 or  Binding<T> = Binding<T>
                    prop.SetValue(instance, parsedValue, null);
                }

                return;
            }


            throw new Exception($"Property {rawTypeName} is not castable to type IParsable<>");
        }

        //public void UpdateProperty(string propertyName, Object value)
        //{
        //    UpdateProperty(this, propertyName, value);
        //}

        public static void UpdateProperty(object instance, string propertyName, Object value)
        {
            if (string.IsNullOrEmpty(propertyName)) { return; }

            var prop = instance.GetType().GetProperty(propertyName);

            if (prop == null) { throw new ArgumentException($"Property {propertyName} not found on Type {instance.GetType().FullName}"); }

            string propName = prop.PropertyType.Name;
            string valueTypeName = value.GetType().Name;

            if (propName == "Binding`1" && valueTypeName != "Binding`1")
            {
                // Convert to Binding<T>
                Type bindingGenericType = typeof(Binding<>).MakeGenericType(value.GetType());
                var bindingInstance = Activator.CreateInstance(bindingGenericType, value);

                prop.SetValue(instance, bindingInstance, null);
            }
            else
            {
                // Direct Assignment. e.g. Binding<T> = Binding<T> or Color=Color etc.
                prop.SetValue(instance, value, null);
            }
        }

        //public void UpdateProperty<T>(string propertyName, T value)
        //{
        //    if (string.IsNullOrEmpty(propertyName)) { return; }

        //    //this.GetType().GetProperty(propertyName)?.SetValue(this, value, null);

        //    var prop = this.GetType().GetProperty(propertyName);

        //    if (prop == null) { throw new ArgumentException($"Property {propertyName} not found on Type {this.GetType().FullName}"); }

        //    string propName = prop.PropertyType.Name;
        //    string valueTypeName = typeof(T).Name;

        //    if (propName == "Binding`1" && valueTypeName != "Binding`1")
        //    {
        //        // Convert to Binding<T>
        //        prop.SetValue(this, new Binding<T>(value), null);
        //    }
        //    else
        //    {
        //        // Direct Assignment. e.g. Binding<T> = Binding<T> or Color=Color etc.
        //        prop.SetValue(this, value, null);
        //    }
        //}


    }
}
