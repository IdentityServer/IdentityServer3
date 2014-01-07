using System;
using System.Security.Claims;
using System.Security.Principal;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.IdentityServer.Core
{
    public static class PrincipalExtensions
    {
        public static DateTime GetAuthenticationTime(this IPrincipal principal)
        {
            return principal.GetAuthenticationTimeEpoch().ToDateTimeFromEpoch();
        }
        
        public static long GetAuthenticationTimeEpoch(this IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            var value = cp.FindFirst(Constants.ClaimTypes.AuthenticationTime).Value;

            return long.Parse(value);
        }

        public static string GetSubject(this IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            var value = cp.FindFirst(Constants.ClaimTypes.Subject).Value;

            return value;
        }

        public static string GetAuthenticationMethod(this IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            var value = cp.FindFirst(Constants.ClaimTypes.AuthenticationMethod).Value;

            return value;
        }
    }
}
