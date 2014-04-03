/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerCoreOptions
    {
        public IdentityServerServiceFactory Factory { get; set; }
        public AuthenticationOptions AuthenticationOptions { get; set; }
    }
}