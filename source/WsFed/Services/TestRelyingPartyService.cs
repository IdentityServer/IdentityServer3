/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.WsFed.Models;
using System.Linq;

namespace Thinktecture.IdentityServer.WsFed.Services
{
    public class TestRelyingPartyService : IRelyingPartyService
    {
        IEnumerable<RelyingParty> _rps = new List<RelyingParty>
        {
            new RelyingParty
            {
                Realm = "urn:testrp",
                Enabled = true,
                ReplyUrl = "https://web.local/idsrvrp/",
                TokenType = Thinktecture.IdentityModel.Constants.TokenTypes.Saml2TokenProfile11,
                TokenLifeTime = 1,

                ClaimMappings = new Dictionary<string,string>
                {
                    { "sub", ClaimTypes.NameIdentifier },
                    { "name", ClaimTypes.Name },
                    { "given_name", ClaimTypes.GivenName },
                    { "email", ClaimTypes.Email }
                }
            },
            new RelyingParty
            {
                Realm = "urn:owinrp",
                Enabled = true,
                ReplyUrl = "http://localhost:10313/",
                TokenType = Thinktecture.IdentityModel.Constants.TokenTypes.Saml2TokenProfile11,
                TokenLifeTime = 1,

                ClaimMappings = new Dictionary<string, string>
                {
                    { "sub", ClaimTypes.NameIdentifier },
                    { "name", ClaimTypes.Name },
                    { "given_name", ClaimTypes.GivenName },
                    { "email", ClaimTypes.Email }
                }
            }
        };

        public Task<RelyingParty> GetByRealmAsync(string realm)
        {
            return Task.FromResult(_rps.FirstOrDefault(rp => rp.Realm == realm));
        }
    }
}