/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Owin;
using System;
using Thinktecture.IdentityServer.Core.Notifications;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerOptions
    {
        public IdentityServerServiceFactory Factory { get; set; }
        public AuthenticationOptions AuthenticationOptions { get; set; }
        
        public Action<IAppBuilder, string> AdditionalIdentityProviderConfiguration { get; set; }
        public Action<IAppBuilder, IdentityServerOptions> ConfigurePlugins { get; set; }


        /// <summary>
        /// Gets or sets the <see cref="IdentityServerNotifications"/> to be notify when important events occure
        /// Current Support for when users are created.
        /// </summary>
        public IdentityServerNotifications Notifications { get; set; }
    }
}