/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    public class KatanaDependencyResolver : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var scope = request.GetOwinEnvironment().GetLifetimeScope();
            if (scope != null)
            {
                request.Properties[HttpPropertyKeys.DependencyScope] = new AutofacScope(scope);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}