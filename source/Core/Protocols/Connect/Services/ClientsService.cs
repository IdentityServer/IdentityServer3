using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Repositories;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public class ClientsService : IClientsService
    {
        IOidcClientsRepository _repository;

        public ClientsService(IOidcClientsRepository repository)
        {
            _repository = repository;
        }

        public Client FindById(string clientId)
        {
            return _repository.FindById(clientId);
        }
    }
}
