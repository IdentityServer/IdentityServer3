using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core;

namespace UnitTests.Plumbing
{
    static class ClientFactory
    {
        public static ClaimsPrincipal CreateClient(string clientId, string secret = "secret")
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Id, clientId),
                new Claim(Constants.ClaimTypes.Secret, secret)
            };

            var id = new ClaimsIdentity(claims, "Test");
            return new ClaimsPrincipal(id);
        }
    }
}
