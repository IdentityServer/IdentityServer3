using System;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public class WsFederationPluginOptions
    {
        public const string CookieName = "IdSvr.WsFedTracking";

        public string LogoutUrl
        {
            get
            {
                return MapPath + "/signout";
            }
        }
        
        public IdentityServerOptions IdentityServerOptions { get; set; }
        public WsFederationServiceFactory Factory { get; set; }
        
        public IDataProtector DataProtector
        {
            get
            {
                return IdentityServerOptions.DataProtector;
            }
        }

        public string MapPath { get; set; }

        public WsFederationPluginOptions()
        {
            MapPath = "/wsfed";
        }

        public void Validate()
        {
            if (Factory == null)
            {
                throw new ArgumentNullException("Factory not configured");
            }
            if (DataProtector == null)
            {
                throw new ArgumentNullException("DataProtector not configured");
            }
            if (IdentityServerOptions == null)
            {
                throw new ArgumentNullException("Options not configured");
            }
        }
    }
}