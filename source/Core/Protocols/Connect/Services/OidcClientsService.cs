using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Repositories;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public class OidcClientsService : IOidcClientsService
    {
        IOidcClientsRepository _repository;

        public OidcClientsService(IOidcClientsRepository repository)
        {
            _repository = repository;
        }

        public OidcClient FindById(string clientId)
        {
            return _repository.FindById(clientId);
        }
    }
}
