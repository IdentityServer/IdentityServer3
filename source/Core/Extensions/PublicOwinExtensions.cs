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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Models;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Extensions
{
    /// <summary>
    /// Extension methods for the OWIN environment.
    /// </summary>
    public static class PublicOwinExtensions
    {
        /// <summary>
        /// Gets the public host name for IdentityServer.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <returns></returns>
        public static string GetIdentityServerHost(this IDictionary<string, object> env)
        {
            return env[Constants.OwinEnvironment.IdentityServerHost] as string;
        }

        /// <summary>
        /// Gets the base path of IdentityServer. Can be used inside of Katana <c>Map</c>ped middleware.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetIdentityServerBasePath(this IDictionary<string, object> env)
        {
            return env[Constants.OwinEnvironment.IdentityServerBasePath] as string;
        }

        /// <summary>
        /// Gets the public base URL for IdentityServer.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetIdentityServerBaseUrl(this IDictionary<string, object> env)
        {
            return env.GetIdentityServerHost() + env.GetIdentityServerBasePath();
        }

        /// <summary>
        /// Gets the URL for the logout page.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetIdentityServerLogoutUrl(this IDictionary<string, object> env)
        {
            return env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Logout;
        }

        /// <summary>
        /// Gets the display name of the current user.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetCurrentUserDisplayName(this IDictionary<string, object> env)
        {
            return new OwinContext(env).GetCurrentUserDisplayName();
        }

        /// <summary>
        /// Creates and writes the signin cookie to the response and returns the associated URL to the login page.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <param name="message">The signin message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// env
        /// or
        /// message
        /// </exception>
        public static string CreateSignInRequest(this IDictionary<string, object> env, SignInMessage message)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (message == null) throw new ArgumentNullException("message");

            var options = env.ResolveDependency<IdentityServerOptions>();
            var cookie = new MessageCookie<SignInMessage>(env, options);
            var id = cookie.Write(message);

            var url = env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Login;
            var uri = new Uri(url.AddQueryString("signin=" + id));

            return uri.AbsoluteUri;
        }

        /// <summary>
        /// Issues the login cookie for IdentityServer.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <param name="login">The login information.</param>
        /// <exception cref="System.ArgumentNullException">
        /// env
        /// or
        /// login
        /// </exception>
        public static void IssueLoginCookie(this IDictionary<string, object> env, AuthenticatedLogin login)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (login == null) throw new ArgumentNullException("login");

            var options = env.ResolveDependency<IdentityServerOptions>();
            var sessionCookie = env.ResolveDependency<SessionCookie>();
            var context = new OwinContext(env);

            var props = new AuthenticationProperties();

            // if false, then they're explicit in preventing a persistent cookie
            if (login.PersistentLogin != false)
            {
                if (login.PersistentLogin == true || options.AuthenticationOptions.CookieOptions.IsPersistent)
                {
                    props.IsPersistent = true;
                    if (login.PersistentLogin == true)
                    {
                        var expires = login.PersistentLoginExpiration ?? DateTimeHelper.UtcNow.Add(options.AuthenticationOptions.CookieOptions.RememberMeDuration);
                        props.ExpiresUtc = expires;
                    }
                }
            }

            var authenticationMethod = login.AuthenticationMethod;
            var identityProvider = login.IdentityProvider ?? Constants.BuiltInIdentityProvider;
            if (String.IsNullOrWhiteSpace(authenticationMethod))
            {
                if (identityProvider == Constants.BuiltInIdentityProvider)
                {
                    authenticationMethod = Constants.AuthenticationMethods.Password;
                }
                else
                {
                    authenticationMethod = Constants.AuthenticationMethods.External;
                }
            }

            var user = IdentityServerPrincipal.Create(login.Subject, login.Name, authenticationMethod, identityProvider, Constants.PrimaryAuthenticationType);
            var identity = user.Identities.First();

            var claims = login.Claims;
            if (claims != null && claims.Any())
            {
                claims = claims.Where(x => !Constants.OidcProtocolClaimTypes.Contains(x.Type));
                claims = claims.Where(x => x.Type != Constants.ClaimTypes.Name);
                identity.AddClaims(claims);
            }

            context.Authentication.SignIn(props, identity);
            sessionCookie.IssueSessionId(login.PersistentLogin, login.PersistentLoginExpiration);
        }

        /// <summary>
        /// Gets the sign in message.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <param name="id">The signin identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// env
        /// or
        /// id
        /// </exception>
        public static SignInMessage GetSignInMessage(this IDictionary<string, object> env, string id)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

            var options = env.ResolveDependency<IdentityServerOptions>();
            var cookie = new MessageCookie<SignInMessage>(env, options);

            return cookie.Read(id);
        }
        
        /// <summary>
        /// Gets the current fully logged in IdentityServer user. Returns null if the user is not logged in.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">env</exception>
        public static async Task<ClaimsIdentity> GetIdentityServerFullLogin(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            var context = new OwinContext(env);
            return await context.GetIdentityFrom(Constants.PrimaryAuthenticationType);
        }
        
        /// <summary>
        /// Gets the current partial logged in IdentityServer user. Returns null if the user is not logged in.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">env</exception>
        public static async Task<ClaimsIdentity> GetIdentityServerPartialLogin(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            var context = new OwinContext(env);
            return await context.GetIdentityFrom(Constants.PartialSignInAuthenticationType);
        }

        /// <summary>
        /// Gets the current request identifier.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetRequestId(this IDictionary<string, object> env)
        {
            object value;
            if (env.TryGetValue(Constants.OwinEnvironment.RequestId, out value))
            {
                return value as string;
            }

            return null;
        }
    }
}