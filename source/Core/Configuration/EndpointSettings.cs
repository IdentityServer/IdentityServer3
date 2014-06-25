/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class EndpointSettings
    {
        public EndpointSettings()
        {
            Enabled = false;
        }

        public bool Enabled { get; set; }
    }
}