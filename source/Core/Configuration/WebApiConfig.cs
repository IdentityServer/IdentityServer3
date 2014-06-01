/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Thinktecture.IdentityServer.Core.Plumbing;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityServerCoreOptions options)
        {
            var config = new HttpConfiguration();

            config.EnableCors();

            config.MapHttpAttributeRoutes();
            config.SuppressDefaultHostAuthentication();

            config.MessageHandlers.Insert(0, new KatanaDependencyResolver());
            config.Services.Add(typeof(IExceptionLogger), new IdentityServerExceptionLogger());

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            //var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            //config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            return config;
        }
    }
}