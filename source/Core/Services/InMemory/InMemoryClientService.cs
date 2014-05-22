using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryClientService : IClientService
    {
        IEnumerable<Models.Client> clients;
        public InMemoryClientService(IEnumerable<Models.Client> clients)
        {
            this.clients = clients;
        }

        public Task<Models.Client> FindClientByIdAsync(string clientId)
        {
            var query =
                from client in this.clients
                where client.ClientId == clientId && client.Enabled
                select client;
            
            return Task.FromResult(query.SingleOrDefault());
        }
    }
}
