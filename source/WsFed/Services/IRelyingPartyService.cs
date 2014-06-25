/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Threading.Tasks;
using Thinktecture.IdentityServer.WsFederation.Models;

namespace Thinktecture.IdentityServer.WsFederation.Services
{
    public interface IRelyingPartyService
    {
        Task<RelyingParty> GetByRealmAsync(string realm);
    }
}