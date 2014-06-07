/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.IdentityServer.Core.Models
{
    public abstract class CoreSettings
    {
        public abstract InternalProtectionSettings GetInternalProtectionSettings();
        public abstract X509Certificate2 GetSigningCertificate();
        public abstract string GetIssuerUri();
        public abstract string GetSiteName();
        public abstract string GetPublicHost();
    }
}
