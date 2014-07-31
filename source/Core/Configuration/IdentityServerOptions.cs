/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Owin;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerOptions
    {
        public IdentityServerOptions()
        {
            SiteName = Constants.IdentityServerName;

            this.ProtocolLogoutUrls = new List<string>();
            this.AuthorizeEndpoint = EndpointSettings.Enabled;
            this.TokenEndpoint = EndpointSettings.Enabled;
            this.UserInfoEndpoint = EndpointSettings.Enabled;
            this.DiscoveryEndpoint = EndpointSettings.Enabled;
            this.AccessTokenValidationEndpoint = EndpointSettings.Disabled;
            this.EndSessionEndpoint = EndpointSettings.Enabled;
            this.CspReportEndpoint = EndpointSettings.Disabled;

            this.CorsPolicy = new CorsPolicy();
            this.CookieOptions = new CookieOptions();
        }

        internal void Validate()
        {
            if (IssuerUri.IsMissing())
            {
                throw new ArgumentException("IssuerUri Is Missing");
            }

            if (CookieOptions == null)
            {
                throw new ArgumentException("CookieOptions is missing");
            }
        }

        public string SiteName { get; set; }
        public string IssuerUri { get; set; }

        public string PublicHostName { get; set; }
        public IdentityServerServiceFactory Factory { get; set; }
        public AuthenticationOptions AuthenticationOptions { get; set; }
        public IDataProtector DataProtector { get; set; }

        public CookieOptions CookieOptions { get; set; }
        
        public Action<IAppBuilder, string> AdditionalIdentityProviderConfiguration { get; set; }
        public Action<IAppBuilder, IdentityServerOptions> PluginConfiguration { get; set; }

        public List<string> ProtocolLogoutUrls { get; set; }

        public CorsPolicy CorsPolicy { get; set; }
        
        public EndpointSettings AuthorizeEndpoint { get; set; }
        public EndpointSettings TokenEndpoint { get; set; }
        public EndpointSettings UserInfoEndpoint { get; set; }
        public EndpointSettings DiscoveryEndpoint { get; set; }
        public EndpointSettings AccessTokenValidationEndpoint { get; set; }
        public EndpointSettings EndSessionEndpoint { get; set; }
        public EndpointSettings CspReportEndpoint { get; set; }

        public X509Certificate2 SigningCertificate { get; set; }
        public X509Certificate2 SecondarySigningCertificate { get; set; }
        
        internal IEnumerable<X509Certificate2> PublicKeysForMetadata
        {
            get
            {
                var keys = new List<X509Certificate2>();
                
                if (SigningCertificate != null)
                {
                    keys.Add(SigningCertificate);
                }

                if (SecondarySigningCertificate != null)
                {
                    keys.Add(SecondarySigningCertificate);
                }

                return keys;
            }
        }

    }
}