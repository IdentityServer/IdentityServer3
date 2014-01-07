using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Repositories
{
    public interface IOidcClientsRepository
    {
        OidcClient FindById(string clientId);
    }
}
