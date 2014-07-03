/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    public class KatanaDependencyResolver : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var owin = request.GetOwinContext();
            var scope = owin.Get<ILifetimeScope>("idsrv:AutofacScope");
            if (scope != null)
            {
                request.Properties[HttpPropertyKeys.DependencyScope] = new AutofacScope(scope);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}