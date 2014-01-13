using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Repositories
{
    public interface IClientsRepository
    {
        Client FindById(string clientId);
    }
}
