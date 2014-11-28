﻿/*
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
    /// <summary>
    /// The IdentityServerOptions class is the top level container for all configuration settings of IdentityServer.
    /// </summary>
    public class IdentityServerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerOptions"/> class with default values.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the display name of the site used in standard views.
        /// </summary>
        /// <value>
        /// Display name of the site used in standard views.
        /// </value>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the unique name of this server instance, e.g. https://myissuer.com
        /// </summary>
        /// <value>
        /// Unique name of this server instance, e.g. https://myissuer.com
        /// </value>
        public string IssuerUri { get; set; }

        /// <summary>
        /// Gets or sets the X.509 certificate (and corresponding private key) for signing security tokens.
        /// </summary>
        /// <value>
        /// The signing certificate.
        /// </value>
        public X509Certificate2 SigningCertificate { get; set; }

        /// <summary>
        /// Gets or sets a secondary certificate that will appear in the discovery document. Can be used to prepare clients for certificate rollover
        /// </summary>
        /// <value>
        /// The secondary signing certificate.
        /// </value>
        public X509Certificate2 SecondarySigningCertificate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is required for IdentityServer. Defaults to `true`.
        /// </summary>
        /// <value>
        ///   <c>true</c> if SSL is required; otherwise, <c>false</c>.
        /// </value>
        public bool RequireSsl { get; set; }

        /// <summary>
        /// Gets or sets the name of the public host.
        /// </summary>
        /// <value>
        /// The name of the public host.
        /// </value>
        public string PublicHostName { get; set; }

        /// <summary>
        /// Gets or sets the identity server factory.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        public IdentityServerServiceFactory Factory { get; set; }

        /// <summary>
        /// Gets or sets the data protector.
        /// </summary>
        /// <value>
        /// The data protector.
        /// </value>
        public IDataProtector DataProtector { get; set; }

        /// <summary>
        /// Gets or sets the endpoint configuration.
        /// </summary>
        /// <value>
        /// The endpoints configuration.
        /// </value>
        public Endpoints Endpoints { get; set; }

        /// <summary>
        /// Gets or sets the authentication options.
        /// </summary>
        /// <value>
        /// The authentication options.
        /// </value>
        public AuthenticationOptions AuthenticationOptions { get; set; }

        /// <summary>
        /// Gets or sets the plugin configuration.
        /// </summary>
        /// <value>
        /// The plugin configuration.
        /// </value>
        public Action<IAppBuilder, IdentityServerOptions> PluginConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the CORS policy.
        /// </summary>
        /// <value>
        /// The CORS policy.
        /// </value>
        public CorsPolicy CorsPolicy { get; set; }

        /// <summary>
        /// Gets or sets the protocol logout urls.
        /// </summary>
        /// <value>
        /// The protocol logout urls.
        /// </value>
        public List<string> ProtocolLogoutUrls { get; set; }

        /// <summary>
        /// Gets or sets the CSP options.
        /// </summary>
        /// <value>
        /// The CSP options.
        /// </value>
        public CspOptions CspOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether web API diagnostics should be enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if web API diagnostics should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableWebApiDiagnostics { get; set; }

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