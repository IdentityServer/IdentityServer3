/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFederation.Services;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public class WsFederationServiceFactory
    {
        // mandatory (external)
        public Registration<IUserService> UserService { get; set; }
        public Registration<CoreSettings> CoreSettings { get; set; }
        public Registration<WsFederationSettings> WsFederationSettings { get; set; }
        public Registration<IRelyingPartyService> RelyingPartyService { get; set; }

        public void Validate()
        {
            if (UserService == null) throw new InvalidOperationException("UserService not configured");
            if (WsFederationSettings == null) throw new InvalidOperationException("WsFederationSettings not configured");
            if (CoreSettings == null) throw new InvalidOperationException("CoreSettings not configured");
            if (RelyingPartyService == null) throw new InvalidOperationException("RelyingPartyService not configured");
        }
    }
}