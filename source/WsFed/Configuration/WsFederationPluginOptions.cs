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
        public const string WsFedCookieAuthenticationType = "WsFedSignInOut";

        public PluginDependencies Dependencies { get; set; }
        public Func<IRelyingPartyService> RelyingPartyService { get; set; }
        public bool EnabledFederationMetadata { get; set; }
    }
}
