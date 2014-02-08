
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityServer.Core.Connect.Models;
namespace Thinktecture.IdentityServer.Core.Services
{
    public interface ICoreSettings
    {
        IEnumerable<Scope> GetScopes();
        Client FindClientById(string clientId);
        bool RequiresConsent(string clientId, ClaimsPrincipal user, IEnumerable<string> scopes);

        X509Certificate2 GetSigningCertificate();
        string GetIssuerUri();
        string GetSiteName();
        InternalProtectionSettings GetInternalProtectionSettings();
    }
}
