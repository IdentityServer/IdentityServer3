/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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

            this.RequireSsl = true;
            
            this.Endpoints = new Endpoints();
            this.CorsPolicy = new CorsPolicy();
            this.AuthenticationOptions = new AuthenticationOptions();
            this.CspOptions = new CspOptions();
        }

        internal void Validate()
        {
            if (IssuerUri.IsMissing())
            {
                throw new ArgumentException("IssuerUri Is Missing");
            }
            if (AuthenticationOptions == null)
            {
                throw new ArgumentException("AuthenticationOptions is missing");
            }
            if (CspOptions == null)
            {
                throw new ArgumentException("CspOptions is missing");
            }
            if (Endpoints == null)
            {
                throw new ArgumentException("Endpoints is missing");
            }
        }

        public string SiteName { get; set; }
        public string IssuerUri { get; set; }

        public string PublicHostName { get; set; }
        public IdentityServerServiceFactory Factory { get; set; }
        public AuthenticationOptions AuthenticationOptions { get; set; }
        public IDataProtector DataProtector { get; set; }

        public Endpoints Endpoints { get; set; }
        public bool RequireSsl { get; set; }
        
        public Action<IAppBuilder, IdentityServerOptions> PluginConfiguration { get; set; }

        public List<string> ProtocolLogoutUrls { get; set; }

        public CorsPolicy CorsPolicy { get; set; }

        public CspOptions CspOptions { get; set; }

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