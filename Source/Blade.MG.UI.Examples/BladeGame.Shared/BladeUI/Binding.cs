using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BladeGame.BladeUI
{

    public interface IBinding
    {
        Type BaseType { get; }
        void FromString(string value);
    }


    /// <summary>
    /// var intRef = new Binding<int>(()=>myint, (value)=>myint=value);
    /// if (Text.ToString() == "80") Text.FromString("30");
    /// if (Text.BaseType == typeof(int)) { ((Binding<int>)Text).Value = 30; }
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Binding<T> : IBinding //where T : struct
    {
        protected Func<T> Getter;
        protected Action<T> Setter;


        public T Value { get { return Getter(); } set { Setter(value); } }
        //public T Value { get { return GetValue(); } set { SetValue(value); } }

        private T backingVar;

        /// <summary>
        /// Auto-Generate Backing Member
        /// var Score = new AutoBinding<int>(10);
        /// </summary>
        /// <param name="initialValue"></param>
        public Binding(T initialValue = default(T))
        {
            backingVar = initialValue;
            this.Getter = () => backingVar;
            this.Setter = (value) => backingVar = value;
        }

        /// <summary>
        /// Use another variable as a backing member
        /// int myint = 10;
        /// var intRef = new Reference<int>(() => myint, (value) => myint = value);
        /// </summary>
        public Binding(Func<T> getter, Action<T> setter = null)
        {
            this.Getter = getter;
            this.Setter = setter;
        }

        public Type BaseType => typeof(T);

        //protected virtual T GetValue()
        //{
        //    return Getter();
        //}

        //protected virtual void SetValue(T value)
        //{
        //    Setter(value);
        //}

        public void FromString(string value)
        {
            if (Setter != null)
            {
                Setter((T)Convert.ChangeType(value, typeof(T)));
            }
        }

        public override string ToString()
        {
            return Getter().ToString();
        }

        public static implicit operator Binding<T>(T value)
        {
            return new Binding<T>(value);
        }

        public static explicit operator T(Binding<T> value)
        {
            return value.Value;
        }
    }

    //public class AutoBinding<T> : Binding<T>
    //{
    //    private T backingVar;

    //    /// <summary>
    //    /// Auto-Generate Backing Member
    //    /// var Score = new AutoBinding<int>(10);
    //    /// </summary>
    //    /// <param name="initialValue"></param>
    //    public AutoBinding(T initialValue = default(T))
    //        : base(null, null)
    //    {
    //        backingVar = initialValue;
    //        this.Getter = () => backingVar;
    //        this.Setter = (value) => backingVar = value;
    //    }
    //}

    public class Binding<T1, T2, T3> : Binding<T1> where T3 : IBindingConverter, new()
    {

        protected new Func<T2> Getter;
        protected new Action<T2> Setter;

        //protected override T1 GetValue()
        //{
        //    //object o = Getter();
        //    return (T1)new T3().ConvertFrom(Getter());
        //}

        //protected override void SetValue(T1 value)
        //{
        //    Setter((T2)new T3().ConvertTo(value));
        //}

        public Binding(T2 initialValue = default(T2)) : base((T1)new T3().ConvertFrom((T2)initialValue))
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
}
