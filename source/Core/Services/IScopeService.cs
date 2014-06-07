/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Models
{
    public interface IScopeService
    {
        Task<IEnumerable<Scope>> GetScopesAsync();
    }
}