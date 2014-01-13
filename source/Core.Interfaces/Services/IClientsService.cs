using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IClientsService
    {
        Client FindById(string clientId);
    }
}