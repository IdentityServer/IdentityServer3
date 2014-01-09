using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public class TestConsentService : IConsentService
    {
        public bool RequiresConsent(Client client, List<string> scopes)
        {
            return false;
        }
    }
}
