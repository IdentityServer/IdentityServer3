/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class EndpointSettings
    {
        public static EndpointSettings Disabled
        {
            get { return new EndpointSettings { IsEnabled = false }; }
        }

        public static EndpointSettings Enabled
        {
            get { return new EndpointSettings { IsEnabled = true }; }
        }

        public EndpointSettings()
        {
            IsEnabled = false;
        }

        public bool IsEnabled { get; set; }
    }
}