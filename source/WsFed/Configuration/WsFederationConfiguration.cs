/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Thinktecture.IdentityServer.WsFed.Services;

namespace Thinktecture.IdentityServer.WsFed.Configuration
{
    public class WsFederationConfiguration
    {
        public const string WsFedCookieAuthenticationType = "WsFedSignInOut";

        public bool EnabledFederationMetadata { get; set; }
    }
}