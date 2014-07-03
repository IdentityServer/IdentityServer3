/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Thinktecture.IdentityServer.Core.Hosting;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    internal static class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityServerCoreOptions options)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            config.SuppressDefaultHostAuthentication();

            config.MessageHandlers.Insert(0, new KatanaDependencyResolver());
            config.Services.Add(typeof(IExceptionLogger), new LogProviderExceptionLogger());

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            return config;
        }
    }
}