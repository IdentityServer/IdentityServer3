using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Connect.TestServices
{
    public class TestConsentService : IConsentService
    {
        public bool RequiresConsent(Client client, ClaimsPrincipal user, List<string> scopes)
        {
            return false;
        }
    }
}
