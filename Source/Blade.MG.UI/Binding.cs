using Blade.MG.UI.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI
{

    public interface IBinding
    {
        [JsonIgnore]
        [XmlIgnore]
        Type BaseType { get; }
        void FromString(string value);
    }


    /// <summary>
    /// var intRef = new Binding<int>(()=>myint, (value)=>myint=value);
    /// if (Text.ToString() == "80") Text.FromString("30");
    /// if (Text.BaseType == typeof(int)) { ((Binding<int>)Text).Value = 30; }
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonConverter(typeof(JsonBindingConverter))]
    public class Binding<T> : IBinding //, IXmlSerializable  //where T : struct
    {
        // public readonly string ID = Guid.NewGuid().ToString(); // Debugging - Remove

        protected Func<T> Getter;
        protected Action<T> Setter;


        //public T Value { get { return Getter(); } set { Setter(value); } }
        public T Value { get { return GetValue(); } set { SetValue(value); } }

        private T backingVar;

        public bool IsImplicitCast { get; init; } // True if this Binding<T> was created from an Implicit Cast

        private Binding()
        {

        }

        /// <summary>
        /// Auto-Generate Backing Member
        /// var Score = new AutoBinding<int>(10);
        /// </summary>
        /// <param name="initialValue"></param>
        public Binding(T initialValue = default, bool isCast = false)
        {
            backingVar = initialValue;
            Getter = () => backingVar;
            Setter = (value) => backingVar = value;
            IsImplicitCast = isCast;
        }

        /// <summary>
        /// Use another variable as a backing member
        /// int myint = 10;
        /// var intRef = new Reference<int>(() => myint, (value) => myint = value);
        /// </summary>
        public Binding(Func<T> getter, Action<T> setter = null)
        {
            Getter = getter;
            Setter = setter;
        }

        public void SetFromBinding(Binding<T> binding)
        {
            this.Getter = binding.Getter;
            this.Setter = binding.Setter;
        }

        public Type BaseType => typeof(T);

        protected virtual T GetValue()
        {
            return Getter();
        }

        protected virtual void SetValue(T value)
        {
            Setter(value);
        }

        public void FromString(string value)
        {
            if (Setter != null)
            {
                Setter((T)Convert.ChangeType(value, typeof(T)));
            }
        }

        public override string ToString()
        {
            return Getter()?.ToString() ?? "";
        }


        //XmlSchema IXmlSerializable.GetSchema()
        //{
        //    return null;
        //}

        //void IXmlSerializable.ReadXml(XmlReader reader)
        //{
        //    XmlSerializer keySerializer = new XmlSerializer(typeof(T));

        //    bool wasEmpty = reader.IsEmptyElement;

        //    reader.Read();
        //    if (wasEmpty)
        //    {
        //        return;
        //    }

        //    //reader.ReadStartElement("key");
        //    T key = (T)keySerializer.Deserialize(reader);
        //    //reader.ReadEndElement();

        //    Value = key;

        //    //reader.MoveToContent();
        //    //reader.ReadEndElement();
        //}

        //void IXmlSerializable.WriteXml(XmlWriter writer)
        //{
        //    XmlSerializer keySerializer = new XmlSerializer(typeof(T));
        //    //writer.WriteStartElement("key");
        //    keySerializer.Serialize(writer, Value);
        //    //writer.WriteEndElement();
        //}

        /// <summary>
        /// Implicitly Cast from Type T to Binding<T> on Assignment 
        /// e.g. Binding<bool> boolValue = true;  
        ///      Is Equivalent to:
        ///      Binding<bool> boolValue = new Binding<bool>(true);
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Binding<T>(T value)
        {
            return new Binding<T>(value, true);
        }

        public static implicit operator Binding<T>(Func<T> value)
        {
            return new Binding<T>(value);
        }

        // Testing - Remove it not needed
        public static implicit operator Binding<T>(Style<T> value)
        {
            return new Binding<T>(() => ResourceDict.GetResourceValue<T>("", value));
        }

        /// <summary>
        /// Cast from Binding<T> to T
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator T(Binding<T> value)
        {
            return value.Value;
        }


        //public static explicit operator T(Binding<T> value)
        //{
        //    return value.Value;
        //}


        //public static implicit operator Binding<T>(string value)
        //{
        //    if (typeof(T) == typeof(string))
        //    {
        //        return new Binding<T>((T)((object)value));
        //    }
        //    else
        //    {
        //        return new Binding<T>((T)((object)value));
        //    }
        //}

        private bool hasValue => Value != null;

        public override bool Equals(object other)
        {
            if (!hasValue) return other == null;
            if (other == null) return false;
            return Value.Equals(other);
        }

        public override int GetHashCode() => hasValue ? Value.GetHashCode() : 0;
    }


    public class Binding<T1, T2, T3> : Binding<T1> where T3 : IBindingConverter, new()
    {

        protected new Func<T2> Getter;
        protected new Action<T2> Setter;

        protected override T1 GetValue()
        {
            //object o = Getter();
            return (T1)new T3().ConvertFrom(Getter());
        }

        protected override void SetValue(T1 value)
        {
            Setter((T2)new T3().ConvertTo(value));
        }

        public Binding(T2 initialValue = default) : base((T1)new T3().ConvertFrom(initialValue))
        {
            Getter = () => { return (T2)new T3().ConvertTo(base.Getter()); };
            Setter = (value) => { base.Setter((T1)new T3().ConvertFrom(value)); };

        }

        /// <summary>
        /// Use another variable as a backing member
        /// int myint = 10;
        /// var intRef = new Reference<int>(() => myint, (value) => myint = value);
        /// </summary>
        public Binding(Func<T2> getter, Action<T2> setter = null) : base(null, null)
        {
            //#if NETFX_CORE
            if (typeof(IBinding).GetTypeInfo().IsAssignableFrom(typeof(T1).GetTypeInfo()))
            {
                throw new NotSupportedException("You cannot bind to a Type which implements IBindable : " + typeof(T1).ToString());
            }
            if (typeof(IBinding).GetTypeInfo().IsAssignableFrom(typeof(T2).GetTypeInfo()))
            {
                throw new NotSupportedException("You cannot bind to a Type which implements IBindable : " + typeof(T2).ToString());
            }
            //#else
            //            if (typeof(IBinding).IsAssignableFrom(typeof(T1)))
            //            {
            //            throw new NotSupportedException("You cannot bind to a Type which implements IBindable : " + typeof(T1).ToString());
            //            }
            //             if (typeof(IBinding).IsAssignableFrom(typeof(T2)))
            //            {
            //            throw new NotSupportedException("You cannot bind to a Type which implements IBindable : " + typeof(T2).ToString());
            //            }
            //#endif

            Getter = () => { return getter(); };
            Setter = (value) => { setter(value); };

            base.Getter = () => { return (T1)new T3().ConvertFrom(Getter()); };
            base.Setter = (value) => { Setter((T2)new T3().ConvertTo(value)); };

        }

    }

    public interface IBindingConverter
    {
        object ConvertFrom(object o);
        object ConvertTo(object o);
    }

    public class IntToStringBindingConverter : IBindingConverter
    {
        public object ConvertFrom(object i)
        {
            if (i is int)
            {
                return ((int)i).ToString();
            }
            else if (i is Binding<int>)
            {
                return ((Binding<int>)i).Value.ToString();
            }
            else
            {
                return null;
            }
        }

        public object ConvertTo(object s)
        {
            return int.Parse(s as string);
        }
    }

    public class FloatToStringBindingConverter : IBindingConverter
    {
        public object ConvertFrom(object i)
        {
            if (i is float)
            {
                return ((float)i).ToString("F2");
            }
            else if (i is Binding<float>)
            {
                return ((Binding<float>)i).Value.ToString("F2");
            }
            else
            {
                return null;
            }
        }

        public object ConvertTo(object s)
        {
            return float.Parse(s as string);
        }
    }


    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-8-0
    public class JsonBindingConverter : JsonConverterFactory
    {
        //public override bool CanConvert(Type objectType)
        //{
        //    return true;
        //}

        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    return new object();
        //}

        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    writer.WriteValue(value.ToString());
        //}
        public override bool CanConvert(Type typeToConvert)
        {
            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type[] typeArguments = typeToConvert.GetGenericArguments();
            Type keyType = typeArguments[0];
            //Type valueType = typeArguments[1];

            // Return a converter for the wrapped type
            //return options.GetConverter(keyType);

            //JsonSerializer.Serialize(keyType);

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(BindingConverterInner<>).MakeGenericType([keyType]),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: [options],
                culture: null)!;

            //JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            //    typeof(DictionaryEnumConverterInner<,>).MakeGenericType([keyType, valueType]),
            //    BindingFlags.Instance | BindingFlags.Public,
            //    binder: null,
            //    args: [options],
            //    culture: null)!;

            return converter;
        }


        private class BindingConverterInner<TKey> : JsonConverter<Binding<TKey>>
        {
            private readonly JsonConverter<TKey> _valueConverter;

            public BindingConverterInner(JsonSerializerOptions options)
            {
                // For performance, use the existing converter.
                _valueConverter = (JsonConverter<TKey>)options.GetConverter(typeof(TKey));
            }

            public override Binding<TKey> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, Binding<TKey> value, JsonSerializerOptions options)
            {
                _valueConverter.Write(writer, value.Value, options);
            }
        }

    }

}
