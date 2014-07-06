using System;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public abstract class Registration
    {
        public static Registration<T> RegisterType<T>(Type type)
            where T: class
        {
            return new Registration<T>
            {
                Type = type
            };
        }

        public static Registration<T> RegisterFactory<T>(Func<T> typeFunc)
            where T : class
        {
            return new Registration<T>
            {
                TypeFactory = typeFunc
            };
        }

        internal abstract Type InterfaceType { get; }
        internal abstract Type ImplementationType { get; }
        internal abstract Func<object> ImplementationFactory { get; }
    }

    public class Registration<T> : Registration
        where T : class
    {
        public Type Type { get; set; }
        public Func<T> TypeFactory { get; set; }

        internal override Type InterfaceType
        {
            get { return typeof(T); }
        }

        internal override Type ImplementationType
        {
            get { return Type; }
        }

        internal override Func<object> ImplementationFactory
        {
            get { return TypeFactory; }
        }
    }
}