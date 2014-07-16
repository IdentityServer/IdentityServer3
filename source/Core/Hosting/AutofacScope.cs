/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    public class AutofacScope : IDependencyScope
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

            Type type = typeof(IEnumerable<>).MakeGenericType(new Type[] { serviceType });
            return (IEnumerable<object>)_scope.Resolve(type);
        }

        public void Dispose()
        {
        }
    }
}