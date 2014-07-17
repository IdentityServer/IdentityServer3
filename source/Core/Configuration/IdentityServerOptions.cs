/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Owin;
using System;
using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerOptions
    {
        public IdentityServerOptions()
        {
            this.ProtocolLogoutUrls = new List<string>();
        }

        public string PublicHostName { get; set; }
        public IdentityServerServiceFactory Factory { get; set; }
        public AuthenticationOptions AuthenticationOptions { get; set; }
        public virtual IDataProtector DataProtector { get; set; }
        
        public Action<IAppBuilder, string> AdditionalIdentityProviderConfiguration { get; set; }
        public Action<IAppBuilder, IdentityServerOptions> ConfigurePlugins { get; set; }

        public List<string> ProtocolLogoutUrls { get; set; }
    }
}