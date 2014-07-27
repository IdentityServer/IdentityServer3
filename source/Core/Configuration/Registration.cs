/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public abstract class Registration
    {
        public static Registration<T> RegisterType<T>(Type type)
            where T: class
        {
            if (type == null) throw new ArgumentNullException("type");

            return new Registration<T>
            {
                Type = type
            };
        }

        public static Registration<T> RegisterFactory<T>(Func<T> typeFunc)
            where T : class
        {
            if (typeFunc == null) throw new ArgumentNullException("typeFunc");

            return new Registration<T>
            {
                TypeFactory = typeFunc
            };
        }
        
        public static Registration<T> RegisterSingleton<T>(T instance)
            where T : class
        {
            if (instance == null) throw new ArgumentNullException("instance");
            return RegisterFactory<T>(() => instance);
        }

        public abstract Type InterfaceType { get; }
        public abstract Type ImplementationType { get; }
        public abstract Func<object> ImplementationFactory { get; }
    }

    public class Registration<T> : Registration
        where T : class
    {
        public Type Type { get; set; }
        public Func<T> TypeFactory { get; set; }

        public override Type InterfaceType
        {
            get { return typeof(T); }
        }

        public override Type ImplementationType
        {
            get { return Type; }
        }

        public override Func<object> ImplementationFactory
        {
            get { return TypeFactory; }
        }
    }
}