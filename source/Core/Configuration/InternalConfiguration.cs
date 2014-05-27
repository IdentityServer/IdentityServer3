/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class InternalConfiguration
    {
        public PluginConfiguration PluginDependencies { get; set; }

        public InternalConfiguration()
        {
            PluginDependencies = new PluginConfiguration();
        }
    }
}
