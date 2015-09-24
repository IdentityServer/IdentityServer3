/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

namespace IdentityServer3.Core.Configuration.Hosting
{
    internal class AutofacScope : IDependencyScope
    {
        private readonly ILifetimeScope _scope;

        public AutofacScope(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public object GetService(Type serviceType)
        {
            return _scope.ResolveOptional(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (!_scope.IsRegistered(serviceType))
            {
                return Enumerable.Empty<object>();
            }

            Type type = typeof(IEnumerable<>).MakeGenericType(serviceType);
            return (IEnumerable<object>)_scope.Resolve(type);
        }

        public void Dispose()
        {
        }
    }
}