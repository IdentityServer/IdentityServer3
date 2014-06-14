/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class InternalConfiguration
    {
        public PluginConfiguration PluginConfiguration { get; set; }
        public IDataProtector DataProtector { get; set; }

        public InternalConfiguration()
        {
            PluginConfiguration = new PluginConfiguration();
        }
    }
}
