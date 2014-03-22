using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.IdentityServer.Core
{
    public static class PrincipalExtensions
    {
        [DebuggerStepThrough]
        public static DateTime GetAuthenticationTime(this IPrincipal principal)
        {
            return principal.GetAuthenticationTimeEpoch().ToDateTimeFromEpoch();
        }

        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            var value = cp.FindFirst(Constants.ClaimTypes.AuthenticationTime).Value;

            return long.Parse(value);
        }

        [DebuggerStepThrough]
        public static string GetSubjectId(this IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            var value = cp.FindFirst(Constants.ClaimTypes.Subject).Value;

            return value;
        }

        [DebuggerStepThrough]
        public static string GetName(this IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            var value = cp.FindFirst(Constants.ClaimTypes.Name).Value;

            return value;
        }

        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            var value = cp.FindFirst(Constants.ClaimTypes.AuthenticationMethod).Value;

            return value;
        }
    }
}