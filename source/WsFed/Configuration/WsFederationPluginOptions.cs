using System;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public class WsFederationPluginOptions
    {
        public const string CookieName = "WsFedTracking";

        public WsFederationServiceFactory Factory { get; set; }

        public void Validate()
        {
            if (Factory == null)
            {
                throw new ArgumentNullException("Factory");
            }
        }
    }
}