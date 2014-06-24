/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Autofac;
using Autofac.Integration.WebApi;
using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public static class AutofacConfig
    {
        public static IContainer Configure(IdentityServerServiceFactory factory, InternalConfiguration internalConfig)
        {
            if (internalConfig == null) throw new ArgumentNullException("internalConfig");
            if (factory == null) throw new ArgumentNullException("factory");
            factory.Validate();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(internalConfig).AsSelf();

            // mandatory from factory
            builder.Register(ctx => factory.CoreSettings()).As<CoreSettings>();
            builder.Register(ctx => factory.UserService()).As<IUserService>();
            
            
            // validators
            
            // processors
            
            // general services
            builder.RegisterType<CookieMiddlewareTrackingCookieService>().As<ITrackingCookieService>();

            // load core controller
            builder.RegisterApiControllers(typeof(WsFederationController).Assembly);

            return builder.Build();
        }
    }
}