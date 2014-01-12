using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface IClientsService
    {
        Client FindById(string clientId);
    }
}