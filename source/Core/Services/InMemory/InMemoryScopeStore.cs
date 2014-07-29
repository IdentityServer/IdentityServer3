/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryScopeStore : IScopeStore
    {
        readonly IEnumerable<Scope> _scopes;

        public InMemoryScopeStore(IEnumerable<Scope> scopes)
        {
            _scopes = scopes;
        }

        public Task<IEnumerable<Scope>> GetScopesAsync()
        {
            return Task.FromResult(_scopes);
        }
    }
}