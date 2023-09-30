//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Blade.UI.Components
//{
//    public static class ConvertExt
//    {
//        private static Dictionary<Type, Func<string, object>> converters = new Dictionary<Type, Func<string, object>>();

//        static ConvertExt()
//        {
//            converters.Add(typeof(float), ToFloat);
//            converters.Add(typeof(Int32), ToInt32);
//            converters.Add(typeof(Type), ToType);
//            converters.Add(typeof(String), ToStringType);
//            //converters.Add(typeof(System.Windows.GridLength), ToGridLength);
//        }

//        public static T ParseValue<T>(object value)
//        {
//            return (T)ParseValue(typeof(T), value);
//        }

//        public static Object ParseValue(Type destType, object value)
//        {
//            Func<string, object> converter;
//            if (converters.TryGetValue(destType, out converter))
//            {
//                return converter(Convert.ToString(value));
//            }
//            else
//            {
//                return value;
//            }
//        }

//        private static object ToFloat(string value)
//        {
//            if (String.IsNullOrEmpty(value) || String.Equals(value, "Auto", StringComparison.CurrentCultureIgnoreCase)) //.InvariantCultureIgnoreCase))
//            {
//                return float.NaN;
//            }

//            return (float)Convert.ToDouble(value);
//        }

//        private static object ToInt32(string value)
//        {
//            if (String.IsNullOrEmpty(value) || String.Equals(value, "Auto", StringComparison.CurrentCultureIgnoreCase)) //.InvariantCultureIgnoreCase))
//            {
//                return 0; // Default value ??
//            }

//            return Convert.ToInt32(value);
//        }

//        private static object ToStringType(string value)
//        {
//            return value;
//        }

//        private static object ToType(string value)
//        {
//            Type testType = Type.GetType("Blade.Xaml.Model.Controls." + value, true);
//            return testType;
//        }

//        //private static object ToGridLength(string value)
//        //{
//        //    if (string.IsNullOrEmpty(value))
//        //    {
//        //        return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star); // What's the default ?
//        //    }

//        //    if (value.Equals("Auto", StringComparison.InvariantCultureIgnoreCase))
//        //    {
//        //        return new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto);
//        //    }
//        //    else if (value.EndsWith("*"))
//        //    {
//        //        value = value.Substring(0, value.Length - 1);
//        //        float amount = float.Parse(value);
//        //        return new System.Windows.GridLength(amount, System.Windows.GridUnitType.Star);
//        //    }
//        //    else
//        //    {
//        //        float amount = float.Parse(value);
//        //        return new System.Windows.GridLength(amount, System.Windows.GridUnitType.Pixel);
//        //    }

//        //}

//    }

//}
