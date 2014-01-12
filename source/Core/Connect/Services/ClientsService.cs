using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Repositories;

namespace Thinktecture.IdentityServer.Core.Connect.Services
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
