using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.WsFederation.Services;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public class WsFederationPluginOptions
    {
        public const string CookieName = "WsFedTracking";

        public WsFederationPluginOptions()
        {
            EnableFederationMetadata = true;
        }

        public IdentityServerServiceFactory Factory { get; set; }

        public Func<IRelyingPartyService> RelyingPartyService { get; set; }
        public bool EnableFederationMetadata { get; set; }

        public void Validate()
        {
            if (RelyingPartyService == null)
            {
                throw new ArgumentNullException("RelyingPartyService");
            }

            if (Factory == null)
            {
                throw new ArgumentNullException("Factory");
            }
        }
    }
}