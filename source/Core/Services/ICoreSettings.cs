/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
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
