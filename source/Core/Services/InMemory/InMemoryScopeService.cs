/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryScopeService : IScopeService
    {
        IEnumerable<Models.Scope> _scopes;

        public InMemoryScopeService(IEnumerable<Models.Scope> scopes)
        {
            this._scopes = scopes;
        }

        public Task<IEnumerable<Models.Scope>> GetScopesAsync()
        {
            return Task.FromResult(_scopes);
        }
    }
}