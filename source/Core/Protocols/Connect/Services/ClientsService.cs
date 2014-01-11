using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Repositories;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public class ClientsService : IClientsService
    {
        IClientsRepository _repository;

        public ClientsService(IClientsRepository repository)
        {
            _repository = repository;
        }

        public Client FindById(string clientId)
        {
            return _repository.FindById(clientId);
        }
    }
}
