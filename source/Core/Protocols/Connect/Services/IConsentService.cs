
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public interface IConsentService
    {
        bool RequiresConsent(OidcClient client, List<string> scopes);
    }
}
