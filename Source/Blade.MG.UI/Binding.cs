#nullable enable

using Blade.MG.UI.Models;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blade.MG.UI
{

    public interface IBinding
    {
        [JsonIgnore]
        Type BaseType { get; }
        void FromString(string value);

        /// <summary>
        /// Raised whenever Value changes to something not equal to its previous value.
        /// Used to bubble render-cache invalidation up to ancestor controls - see
        /// UIComponent.EnsureBindingsWired/BubbleInvalidation.
        /// </summary>
        event Action? Changed;
    }


    /// <summary>
    /// var intRef = new Binding<int>(()=>myint, (value)=>myint=value);
    /// if (Text.ToString() == "80") Text.FromString("30");
    /// if (Text.BaseType == typeof(int)) { ((Binding<int>)Text).Value = 30; }
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonConverter(typeof(JsonBindingConverter))]
    public class Binding<T> : IBinding
    {
        // Populated only when this Binding relays external/live state (see the Func<T>
        // constructor below) - the overwhelmingly common "just holds a value" case (including
        // every implicit T->Binding<T> cast, e.g. `border.Background = Color.Red;`) leaves these
        // null and reads/writes _backingVar directly instead. Construction used to unconditionally
        // allocate a Getter/Setter closure pair (`() => _backingVar` / `v => _backingVar = v`)
        // even for that plain case - two heap allocations per Binding<T>, every time, for state
        // that's never actually external. GetValue/SetValue below are the only places that need
        // to know which mode a given instance is in.
        private Func<T>? _getter;
        private Action<T>? _setter;
        private T _backingVar;

        public event Action? Changed;

        public T Value
        {
            get { return GetValue(); }
            set
            {
                var old = GetValue();
                SetValue(value);
                if (!EqualityComparer<T>.Default.Equals(old, value))
                {
                    Changed?.Invoke();
                }
            }
        }

        public bool IsImplicitCast { get; init; } // True if this Binding<T> was created from an Implicit Cast

        private Binding() : this(default!, false)
        {

        }

        /// <summary>
        /// Auto-Generate Backing Member
        /// var Score = new AutoBinding<int>(10);
        /// </summary>
        /// <param name="initialValue"></param>
        public Binding(T initialValue = default, bool isCast = false)
        {
            _backingVar = initialValue;
            IsImplicitCast = isCast;
        }

        /// <summary>
        /// Use another variable as a backing member
        /// int myint = 10;
        /// var intRef = new Reference<int>(() => myint, (value) => myint = value);
        /// </summary>
        public Binding(Func<T> getter, Action<T>? setter = null)
        {
            _getter = getter;
            _setter = setter;
        }

        public Type BaseType => typeof(T);

        protected virtual T GetValue()
        {
            return _getter is not null ? _getter() : _backingVar;
        }

        protected virtual void SetValue(T value)
        {
            if (_getter is not null)
            {
                _setter?.Invoke(value);
            }
            else
            {
                _backingVar = value;
            }
        }

        public void FromString(string value)
        {
            // A read-only relay binding (Func<T> getter with no setter, e.g. a computed display
            // value) has nothing to write to - matches the old behavior of checking the Setter
            // field for null, just expressed in terms of the new _getter/_setter representation.
            // Goes through SetValue (protected virtual) rather than a raw field so this still
            // dispatches correctly on a Binding<T1,T2,T3>, whose override converts the parsed T1
            // value into its own T2 storage.
            if (_getter is not null && _setter is null)
            {
                return;
            }

            try
            {
                SetValue((T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture));
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
            {
                // Leave the current value unchanged on an unparsable input.
            }
        }

        public override string ToString()
        {
            return GetValue()?.ToString() ?? "";
        }

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

        private bool hasValue => Value != null;

        public override bool Equals(object? other)
        {
            if (!hasValue) return other == null;
            if (other == null) return false;
            return Value!.Equals(other);
        }

        public override int GetHashCode() => hasValue ? Value!.GetHashCode() : 0;
    }


    public class Binding<T1, T2, T3> : Binding<T1> where T3 : IBindingConverter, new()
    {

        protected new Func<T2> Getter;
        protected new Action<T2>? Setter;

        protected override T1 GetValue()
        {
            return (T1)new T3().ConvertFrom(Getter())!;
        }

        protected override void SetValue(T1 value)
        {
            Setter?.Invoke((T2)new T3().ConvertTo(value));
        }

        public Binding(T2 initialValue = default) : base((T1)new T3().ConvertFrom(initialValue)!)
        {
            // Goes through base.GetValue()/SetValue() (the protected virtual accessors) rather
            // than touching base's own storage directly - Binding<T1> no longer exposes that as
            // a pair of always-allocated Getter/Setter fields (see Binding<T>), so routing
            // through the accessors keeps this correct regardless of how base stores its T1
            // value internally.
            Getter = () => { return (T2)new T3().ConvertTo(base.GetValue()); };
            Setter = (value) => { base.SetValue((T1)new T3().ConvertFrom(value)!); };
        }

        /// <summary>
        /// Use another variable as a backing member
        /// int myint = 10;
        /// var intRef = new Reference<int>(() => myint, (value) => myint = value);
        /// </summary>
        public Binding(Func<T2> getter, Action<T2>? setter = null) : base(default!, false)
        {
            if (typeof(IBinding).IsAssignableFrom(typeof(T1)))
            {
                throw new NotSupportedException("You cannot bind to a Type which implements IBindable : " + typeof(T1).ToString());
            }
            if (typeof(IBinding).IsAssignableFrom(typeof(T2)))
            {
                throw new NotSupportedException("You cannot bind to a Type which implements IBindable : " + typeof(T2).ToString());
            }

            Getter = () => { return getter(); };
            Setter = (value) => { setter?.Invoke(value); };

            // (No base.Getter/base.Setter assignment here - GetValue()/SetValue() above are
            // already overridden and read/write the shadowed Getter/Setter fields directly, so
            // base's own storage is never consulted for a Binding<T1,T2,T3> instance.)
        }

    }

    public interface IBindingConverter
    {
        object? ConvertFrom(object o);
        object ConvertTo(object o);
    }

    public class IntToStringBindingConverter : IBindingConverter
    {
        public object? ConvertFrom(object i)
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
            return int.Parse((string)s);
        }
    }

    public class FloatToStringBindingConverter : IBindingConverter
    {
        public object? ConvertFrom(object i)
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
            return float.Parse((string)s);
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
                // Reads a plain value-backed Binding<TKey> - a $bind path declaration (see
                // UIDocumentSerializer) is resolved one level up, before this converter is ever
                // reached, since only that caller knows about the DataContext to bind against.
                TKey value = _valueConverter.Read(ref reader, typeof(TKey), options);
                return new Binding<TKey>(value);
            }

            public override void Write(Utf8JsonWriter writer, Binding<TKey> value, JsonSerializerOptions options)
            {
                _valueConverter.Write(writer, value.Value, options);
            }
        }

    }

}
