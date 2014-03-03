using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.TestServices
{
    public class TestConsentService : IConsentService
    {
        public bool RequiresConsent(Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            return client.RequireConsent;
        }
        
        public void UpdateConsent(Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            throw new NotImplementedException();
        }
    }
}