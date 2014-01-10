using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Repositories
{
    public interface IClientsRepository
    {
        Client FindById(string clientId);
    }
}
