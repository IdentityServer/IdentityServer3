/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public abstract class WsFederationSettings
    {
        public virtual EndpointSettings MetadataEndpoint
        {
            get { return new EndpointSettings { Enabled = true }; }
        }
    }
}