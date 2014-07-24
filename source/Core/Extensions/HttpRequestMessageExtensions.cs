/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using System.Net.Http;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static ILifetimeScope GetAutofacScope(this HttpRequestMessage request)
        {
            return request.GetOwinEnvironment().GetLifetimeScope();
        }

        public static string GetIdentityServerBaseUrl(this HttpRequestMessage request)
        {
            return request.GetOwinContext().Environment.GetIdentityServerBaseUrl();
        }
    }
}
