

using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public interface IOidcClientsService
    {
        OidcClient FindById(string clientId);
    }
}