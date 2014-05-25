/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryConsentService : IConsentService
    {
        private ConcurrentBag<Consent> _consents = new ConcurrentBag<Consent>();
        
        public Task<bool> RequiresConsentAsync(Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            if (!client.RequireConsent)
            {
                return Task.FromResult(false);
            }

            var orderedScopes = string.Join(" ", scopes.OrderBy(s => s).ToArray());

            var query = from c in _consents
                        where c.ClientId == client.ClientId &&
                              c.Scopes == orderedScopes &&
                              c.Subject == user.GetSubjectId()
                        select c;

            var hit = query.FirstOrDefault();

            return Task.FromResult(hit == null);
        }

        public Task UpdateConsentAsync(Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
        {
            if (client.AllowRememberConsent)
            {
                var consent = new Consent
                {
                    ClientId = client.ClientId,
                    Subject = user.GetSubjectId(),
                    Scopes = string.Join(" ", scopes.OrderBy(s => s).ToArray())
                };

                _consents.Add(consent);
            }

            return Task.FromResult(0);
        }
    }
}