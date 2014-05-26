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
        PluginConfiguration _dependencies;

        public WsFederationPluginOptions(PluginConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("dependencies");
            }
            
            _dependencies = configuration;
        }

        public const string WsFedCookieAuthenticationType = "WsFedSignInOut";

        public PluginConfiguration Configuration
        {
            get
            {
                return _dependencies;
            }
        }

        public Func<IRelyingPartyService> RelyingPartyService { get; set; }
        public bool EnableFederationMetadata { get; set; }
    }
}