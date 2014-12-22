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
        public abstract Type InterfaceType { get; }

        public string Name { get; protected set; }
        
        public object Instance { get; protected set; }
        public Type Type { get; protected set; }
        public Func<IDependencyResolver, object> Factory { get; protected set; }
    }

    public class Registration<T> : Registration
        where T : class
    {
        public override Type InterfaceType
        {
            get { return typeof(T); }
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

        public Registration(Func<IDependencyResolver, T> factory, string name = null)
        {
            if (factory == null) throw new ArgumentNullException("factory");

            this.Factory = factory;
            this.Name = name;
        }

        public Registration(T instance, string name = null)
        {
            if (instance == null) throw new ArgumentNullException("instance");

            this.Instance = instance;
            this.Name = name;
        }
        
        public Registration(Registration<T> registration, string name)
        {
            if (registration == null) throw new ArgumentNullException("registration");
            if (name == null) throw new ArgumentNullException("name");

            this.Type = registration.Type;
            this.Factory = registration.Factory;
            this.Instance = registration.Instance;
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