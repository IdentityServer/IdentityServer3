/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.IdentityServer.Core.Extensions
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
        public static long GetAuthenticationTimeEpoch(this IIdentity identity)
        {
            var cp = identity as ClaimsIdentity;
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
        public static string GetSubjectId(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var value = id.FindFirst(Constants.ClaimTypes.Subject).Value;

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
        public static string GetName(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var value = id.FindFirst(Constants.ClaimTypes.Name).Value;

            return value;
        }

        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IPrincipal principal)
        {
            var cp = principal as ClaimsPrincipal;
            var value = cp.FindFirst(Constants.ClaimTypes.AuthenticationMethod).Value;

            return value;
        }

        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var value = id.FindFirst(Constants.ClaimTypes.AuthenticationMethod).Value;

            return value;
        }

        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var value = id.FindFirst(Constants.ClaimTypes.IdentityProvider).Value;

            return value;
        }
    }
}