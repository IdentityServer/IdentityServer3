/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.WsFed.Models;

namespace Thinktecture.IdentityServer.WsFed.Services
{
    public class TestRelyingPartyService : IRelyingPartyService
    {
        public RelyingParty GetByRealm(string realm)
        {
            return new RelyingParty
            {
                Realm = "urn:testrp",
                Enabled = true,
                ReplyUrl = "https://web.local/idsrvrp/",
                TokenType = Thinktecture.IdentityModel.Constants.TokenTypes.Saml2TokenProfile11,
                TokenLifeTime = 1,

                ClaimMappings = new Dictionary<string,string>
                {
                    { "sub", ClaimTypes.NameIdentifier },
                    { "name", ClaimTypes.Name }
                }
            };
        }
    }
}