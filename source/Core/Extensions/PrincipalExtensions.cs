/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using Thinktecture.IdentityModel.Extensions;
using System.Linq;

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
            return principal.Identity.GetAuthenticationTimeEpoch();
        }
        
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var value = id.FindFirst(Constants.ClaimTypes.AuthenticationTime).Value;

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