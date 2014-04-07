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
    public interface ICoreSettings
    {
        Task<IEnumerable<Scope>> GetScopesAsync();
        Task<Client> FindClientByIdAsync(string clientId);
        
        InternalProtectionSettings GetInternalProtectionSettings();
        X509Certificate2 GetSigningCertificate();
        string GetIssuerUri();
        string GetSiteName();
        string GetPublicHost();
        
    }
}
