/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using System;
using System.Net.Http;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static ILifetimeScope GetAutofacScope(this HttpRequestMessage request)
        {
            var owinContext = request.GetOwinContext();
            var scope = owinContext.Get<ILifetimeScope>("idsrv:AutofacScope");

            return scope;
        }

        public static string GetIdentityServerBaseUrl(this HttpRequestMessage request)
        {
            return request.GetOwinContext().Environment.GetIdentityServerBaseUrl();
        }
    }
}
