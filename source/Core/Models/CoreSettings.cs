/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Models
{
    public abstract class CoreSettings
    {
        public abstract string IssuerUri { get; }

        public virtual X509Certificate2 SigningCertificate
        {
            get { return null; } 
        }

        public virtual string SiteName
        {
            get { return "Thinktecture IdentityServer v3"; }
        }

        public virtual string PublicHostName
        {
            get { return string.Empty; }
        }

        public virtual IDataProtector DataProtector
        {
            get { return null; }
        }

        public virtual EndpointSettings AuthorizeEndpoint
        {
            get { return new EndpointSettings { Enabled = true }; }
        }

        public virtual EndpointSettings TokenEndpoint
        {
            get { return new EndpointSettings { Enabled = true }; }
        }

        public virtual EndpointSettings UserInfoEndpoint
        {
            get { return new EndpointSettings { Enabled = true }; }
        }

        public virtual EndpointSettings DiscoveryEndpoint 
        {
            get { return new EndpointSettings { Enabled = true }; }
        }

        public virtual EndpointSettings AccessTokenValidationEndpoint
        {
            get { return new EndpointSettings { Enabled = true }; }
        }

        public virtual EndpointSettings EndSessionEndpoint
        {
            get { return new EndpointSettings { Enabled = true }; }
        }
    }
}