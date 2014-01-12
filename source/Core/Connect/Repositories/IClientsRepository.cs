using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Repositories
{
    public interface IClientsRepository
    {
        Client FindById(string clientId);
    }
}
