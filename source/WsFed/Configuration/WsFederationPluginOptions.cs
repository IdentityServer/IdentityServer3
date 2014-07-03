using System;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public class WsFederationPluginOptions
    {
        public const string CookieName = "WsFedTracking";

        public WsFederationServiceFactory Factory { get; set; }
        public string MapPath { get; set; }
        public string LoginPageUrl { get; set; }
        public string LogoutPageUrl { get; set; }

        public WsFederationPluginOptions()
        {
            MapPath = "/wsfed";
        }

        public void Validate()
        {
            if (Factory == null)
            {
                throw new ArgumentNullException("Factory");
            }

            if (string.IsNullOrWhiteSpace(LoginPageUrl))
            {
                throw new ArgumentException("LoginPageUrl is not set");
            }

            if (string.IsNullOrWhiteSpace(LogoutPageUrl))
            {
                throw new ArgumentException("LogoutPageUrl is not set");
            }
        }
    }
}