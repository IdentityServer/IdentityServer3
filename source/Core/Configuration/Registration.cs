/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public abstract class Registration
    {
        public string Name { get; protected set; }
        public abstract Type InterfaceType { get; }
        public abstract Type ImplementationType { get; }
        public abstract Func<IDependencyResolver, object> ImplementationFactory { get; }
    }

    public class Registration<T> : Registration
        where T : class
    {
        public Type Type { get; protected set; }
        public Func<IDependencyResolver, T> TypeFactory { get; protected set; }

        public override Type InterfaceType
        {
            get { return typeof(T); }
        }

        public override Type ImplementationType
        {
            get { return Type; }
        }

        public override Func<IDependencyResolver, object> ImplementationFactory
        {
            get { return TypeFactory; }
        }

        public Registration(string name = null)
        {
            this.Type = typeof(T);
            this.Name = name;
        }

        public Registration(Type type, string name = null)
        {
            if (type == null) throw new ArgumentNullException("type");

            this.Type = type;
            this.Name = name;
        }

        public Registration(Func<IDependencyResolver, T> factoryFunc, string name = null)
        {
            if (factoryFunc == null) throw new ArgumentNullException("factoryFunc");

            this.TypeFactory = factoryFunc;
            this.Name = name;
        }

        public Registration(T instance, string name = null)
        {
            if (instance == null) throw new ArgumentNullException("instance");

            this.TypeFactory = resolver => instance;
            this.Name = name;
        }
    }

    public class Registration<T, TImpl> : Registration<T>
        where T : class
        where TImpl : T
    {
        public Registration(string name = null)
            : base(typeof(TImpl), name)
        {
        }
    }
}