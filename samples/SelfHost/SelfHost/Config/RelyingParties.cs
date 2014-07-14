using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.WsFederation.Models;

namespace SelfHost.Config
{
    public class RelyingParties
    {
        public static IEnumerable<RelyingParty> Get()
        {
            return new List<RelyingParty>
            {   
                new RelyingParty
                {
                    Realm = "urn:testrp",
                    Name = "Test RP",
                    Enabled = true,
                    ReplyUrl = "https://web.local/idsrvrp/",
                    TokenType = Thinktecture.IdentityModel.Constants.TokenTypes.Saml2TokenProfile11,
                    TokenLifeTime = 1,

                    ClaimMappings = new Dictionary<string,string>
                    {
                        { "sub", ClaimTypes.NameIdentifier },
                        { "given_name", ClaimTypes.Name },
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
        }
    }
}
