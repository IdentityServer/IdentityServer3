/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Owin;
using System;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerCoreOptions
    {
        public IdentityServerServiceFactory Factory { get; set; }
        public AuthenticationOptions AuthenticationOptions { get; set; }
        
        public Action<IAppBuilder, string> AdditionalIdentityProviderConfiguration { get; set; }
        public Action<IAppBuilder, IdentityServerCoreOptions> ConfigurePlugins { get; set; }
    }
}