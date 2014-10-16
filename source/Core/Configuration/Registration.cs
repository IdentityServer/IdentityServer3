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