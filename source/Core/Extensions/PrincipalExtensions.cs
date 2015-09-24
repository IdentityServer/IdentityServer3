/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using IdentityModel;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace IdentityServer3.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="System.Security.Principal.IPrincipal"/> and <see cref="System.Security.Principal.IIdentity"/> .
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Gets the authentication time.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DateTimeOffset GetAuthenticationTime(this IPrincipal principal)
        {
            return principal.GetAuthenticationTimeEpoch().ToDateTimeOffsetFromEpoch();
        }

        /// <summary>
        /// Gets the authentication epoch time.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationTimeEpoch();
        }

        /// <summary>
        /// Gets the authentication epoch time.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.AuthenticationTime);

            if (claim == null) throw new InvalidOperationException("auth_time is missing.");
           
            return long.Parse(claim.Value);
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IPrincipal principal)
        {
            return principal.Identity.GetSubjectId();
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">sub claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.Subject);

            if (claim == null) throw new InvalidOperationException("sub claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetName(this IPrincipal principal)
        {
            return principal.Identity.GetName();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">name claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetName(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.Name);

            if (claim == null) throw new InvalidOperationException("name claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationMethod();
        }

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">amr claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.AuthenticationMethod);

            if (claim == null) throw new InvalidOperationException("amr claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IPrincipal principal)
        {
            return principal.Identity.GetIdentityProvider();
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">idp claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(Constants.ClaimTypes.IdentityProvider);

            if (claim == null) throw new InvalidOperationException("idp claim is missing");
            return claim.Value;
        }
    }
}