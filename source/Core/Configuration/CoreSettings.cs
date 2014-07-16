/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public abstract class CoreSettings
    {
        public abstract string IssuerUri { get; }

        public virtual Dictionary<string, string> Properties { get; set; }

        public virtual X509Certificate2 SigningCertificate
        {
            get { return null; } 
        }

        public virtual IEnumerable<X509Certificate2> PublicKeysForMetadata
        {
            get 
            {
                var keys = new List<X509Certificate2>();
                var primary = SigningCertificate;

                if (primary != null)
                {
                    keys.Add(primary);
                }

                return keys;
            }
        }

        public virtual string SiteName
        {
            get { return "Thinktecture IdentityServer v3"; }
        }

        public virtual EndpointSettings AuthorizeEndpoint
        {
            get { return EndpointSettings.Enabled; }
        }

        public virtual EndpointSettings TokenEndpoint
        {
            get { return EndpointSettings.Enabled; }
        }

        public virtual EndpointSettings UserInfoEndpoint
        {
            get { return EndpointSettings.Enabled; }
        }

        public virtual EndpointSettings DiscoveryEndpoint 
        {
            get { return EndpointSettings.Enabled; }
        }

        public virtual EndpointSettings AccessTokenValidationEndpoint
        {
            get { return EndpointSettings.Enabled; }
        }

        public virtual EndpointSettings EndSessionEndpoint
        {
            get { return EndpointSettings.Enabled; }
        }

        public virtual EndpointSettings CspReportEndpoint
        {
            get { return EndpointSettings.Disabled; }
        }
    }
}