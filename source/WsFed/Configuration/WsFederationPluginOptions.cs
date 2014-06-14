using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.WsFed.Services;

namespace Thinktecture.IdentityServer.WsFed.Configuration
{
    public class WsFederationPluginOptions
    {
        PluginConfiguration _dependencies;
        public const string CookieName = "WsFedSignInOut";

        public WsFederationPluginOptions(PluginConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("dependencies");
            }
            
            _dependencies = configuration;
            EnableFederationMetadata = true;
        }

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