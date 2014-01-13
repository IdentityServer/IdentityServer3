using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IConsentService
    {
        bool RequiresConsent(Client client, ClaimsPrincipal user, List<string> scopes);
    }
}
