namespace IdentityServer3.Core.Services.Default
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public static class AnonymousClaimsProvider
    {
        public static IEnumerable<Claim> GetAnonymousClaims()
        {
            var deviceAndSubjectId = Guid.NewGuid().ToString();

            var claims = new List<Claim>
                             {
                                 new Claim(Constants.ClaimTypes.AuthenticationMethod, "anon"),
                                 new Claim("did", deviceAndSubjectId),
                                 new Claim(Constants.ClaimTypes.Subject, deviceAndSubjectId)
                             };

            return claims;
        }
    }
}