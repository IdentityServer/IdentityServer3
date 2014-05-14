using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.WsFed.Services;

namespace Thinktecture.IdentityServer.WsFed.Configuration
{
    public class WsFederationPluginOptions
    {
        PluginDependencies _dependencies;

        public WsFederationPluginOptions(PluginDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public const string WsFedCookieAuthenticationType = "WsFedSignInOut";

        public PluginDependencies Dependencies
        {
            get
            {
                return _dependencies;
            }
        }

        public Func<IRelyingPartyService> RelyingPartyService { get; set; }
        public bool EnabledFederationMetadata { get; set; }
    }
}