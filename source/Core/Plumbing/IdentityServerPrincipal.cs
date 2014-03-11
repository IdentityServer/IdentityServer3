using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    public static class IdentityServerPrincipal
    {
        public static ClaimsPrincipal Create(string subject, string authenticationMethod, string idp, string authenticationType = Constants.BuiltInAuthenticationType)
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, subject),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, authenticationMethod),
                new Claim(Constants.ClaimTypes.IdentityProvider, idp),
                new Claim(Constants.ClaimTypes.AuthenticationTime, DateTime.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer)
            };

            var id = new ClaimsIdentity(claims, authenticationType);
            return new ClaimsPrincipal(id);
        }
    }
}
