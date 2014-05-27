/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryClientService : IClientService
    {
        IEnumerable<Models.Client> _clients;

        public InMemoryClientService(IEnumerable<Models.Client> clients)
        {
            this._clients = clients;
        }

        public Task<Models.Client> FindClientByIdAsync(string clientId)
        {
            var query =
                from client in this._clients
                where client.ClientId == clientId && client.Enabled
                select client;
            
            return Task.FromResult(query.SingleOrDefault());
        }
    }
}