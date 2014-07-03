/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

namespace Thinktecture.IdentityServer.WsFederation.Hosting
{
    public class AutofacScope : IDependencyScope
    {
        ILifetimeScope scope;

        public AutofacScope(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public object GetService(System.Type serviceType)
        {
            return scope.ResolveOptional(serviceType);
        }

        public System.Collections.Generic.IEnumerable<object> GetServices(System.Type serviceType)
        {
            if (!scope.IsRegistered(serviceType))
            {
                return Enumerable.Empty<object>();
            }

            Type type = typeof(IEnumerable<>).MakeGenericType(new Type[] { serviceType });
            return (IEnumerable<object>)scope.Resolve(type);
        }

        public void Dispose()
        {
        }
    }
}