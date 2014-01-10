using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public interface IConsentService
    {
        bool RequiresConsent(Client client, ClaimsPrincipal user, List<string> scopes);
    }
}
