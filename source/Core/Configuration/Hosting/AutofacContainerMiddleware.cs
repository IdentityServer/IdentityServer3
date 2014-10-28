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

using Autofac;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    internal class AutofacContainerMiddleware
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
                env.SetLifetimeScope(scope);
                
                await _next(env);
            }
        }
    }
}