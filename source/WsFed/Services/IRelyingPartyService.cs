/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Thinktecture.IdentityServer.WsFed.Models;

namespace Thinktecture.IdentityServer.WsFed.Services
{
    public interface IRelyingPartyService
    {
        RelyingParty GetByRealm(string realm);
    }
}