using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public interface IClientsService
    {
        Client FindById(string clientId);
    }
}