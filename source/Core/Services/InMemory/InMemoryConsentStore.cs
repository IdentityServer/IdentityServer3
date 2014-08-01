/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryConsentStore : IConsentStore
    {
        private readonly ConcurrentBag<Consent> _consents = new ConcurrentBag<Consent>();
        
        public Task<bool> RequiresConsentAsync(string client, string subject, IEnumerable<string> scopes)
        {
            var orderedScopes = string.Join(" ", scopes.OrderBy(s => s).ToArray());

            var query = from c in _consents
                        where c.ClientId == client &&
                              c.Scopes == orderedScopes &&
                              c.Subject == subject
                        select c;

            var hit = query.FirstOrDefault();

            return Task.FromResult(hit == null);
        }

        public Task UpdateConsentAsync(string client, string subject, IEnumerable<string> scopes)
        {
            var orderedScopes = string.Join(" ", scopes.OrderBy(s => s).ToArray());
            
            _consents.Add(new Consent { ClientId = client, Subject = subject, Scopes = orderedScopes });

            return Task.FromResult(0);
        }
    }
}