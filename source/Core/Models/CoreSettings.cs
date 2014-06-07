/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.IdentityServer.Core.Models
{
    public abstract class CoreSettings
    {
        public abstract InternalProtectionSettings GetInternalProtectionSettings();
        public abstract string GetIssuerUri();

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

        public virtual bool EnableDiscoveryEndpoint
        {
            get { return true; }
        }

        public virtual bool EnableAccessTokenValidationEndpoint
        {
            get { return true; }
        }

        public virtual bool EnableIdentityTokenValidationEndpoint
        {
            get { return true; }
        }

        public virtual IEnumerable<string> TokenEndpointAllowedOrigins
        {
            get { return Enumerable.Empty<string>(); }
        }

        public virtual IEnumerable<string> UserInfoEndpointAllowedOrigins
        {
            get { return Enumerable.Empty<string>(); }
        }
    }
}