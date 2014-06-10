/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class InternalConfiguration
    {
        public PluginConfiguration PluginDependencies { get; set; }
        public IDataProtector DataProtector { get; set; }

        public InternalConfiguration()
        {
            PluginDependencies = new PluginConfiguration();
        }
    }
}
