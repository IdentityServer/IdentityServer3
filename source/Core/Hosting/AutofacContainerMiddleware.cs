/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Autofac;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    public class AutofacContainerMiddleware
    {
        readonly private Func<IDictionary<string, object>, Task> _next;
        readonly private IContainer _container;

        public AutofacContainerMiddleware(Func<IDictionary<string, object>, Task> next, IContainer container)
        {
            _next = next;
            _container = container;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);

            // this creates a per-request, disposable scope
            using (var scope = _container.BeginLifetimeScope(b =>
            {
                // this makes owin context resolvable in the scope
                b.RegisterInstance(context).As<IOwinContext>();
            }))
            {
                // this makes scope available for downstream frameworks
                context.Set<ILifetimeScope>("idsrv:AutofacScope", scope);
                await _next(env);
            }
        }
    }
}