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
    public static class OwinEnvironmentExtensions
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
        /// Creates a sign in request.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// env
        /// or
        /// message
        /// </exception>
        public static string CreateSignInRequest(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            return env.CreateSignInRequest(new SignInMessage());
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

            // if there's no return url, then use current request's URL
            if (message.ReturnUrl.IsMissing())
            {
                var ctx = new OwinContext(env);
                message.ReturnUrl = ctx.Request.Uri.AbsoluteUri;
            }
            if (message.ReturnUrl.StartsWith("~/"))
            {
                message.ReturnUrl = message.ReturnUrl.Substring(1);
            }
            if (message.ReturnUrl.StartsWith("/"))
            {
                message.ReturnUrl = env.GetIdentityServerBaseUrl().RemoveTrailingSlash() + message.ReturnUrl;
            }

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
        /// Creates the sign out request.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">env</exception>
        public static string CreateSignOutRequest(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            return env.CreateSignOutRequest(new SignOutMessage());
        }

        /// <summary>
        /// Creates the sign out request.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">message</exception>
        public static string CreateSignOutRequest(this IDictionary<string, object> env, SignOutMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            // if there's no return url, then use current request's URL
            if (message.ReturnUrl.IsMissing())
            {
                var ctx = new OwinContext(env);
                message.ReturnUrl = ctx.Request.Uri.AbsoluteUri;
            }
            if (message.ReturnUrl.StartsWith("~/"))
            {
                message.ReturnUrl = message.ReturnUrl.Substring(1);
            }
            if (message.ReturnUrl.StartsWith("/"))
            {
                message.ReturnUrl = env.GetIdentityServerBaseUrl().RemoveTrailingSlash() + message.ReturnUrl;
            }

            var options = env.ResolveDependency<IdentityServerOptions>();
            var cookie = new MessageCookie<SignOutMessage>(env, options);
            var id = cookie.Write(message);

            var url = env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Logout;
            var uri = new Uri(url.AddQueryString("id=" + id));
            
            return uri.AbsoluteUri;
        }

        /// <summary>
        /// Updates the partial login with the claims provided.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// env
        /// or
        /// claims
        /// </exception>
        /// <exception cref="System.Exception">No partial login</exception>
        public static async Task UpdatePartialLoginClaimsAsync(this IDictionary<string, object> env, IEnumerable<Claim> claims)
        {
            if (env == null) throw new ArgumentNullException("env");
            if (claims == null) throw new ArgumentNullException("claims");

            var context = new OwinContext(env);
            var result = await context.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (result == null || result.Identity == null || result.Identity.IsAuthenticated == false)
            {
                throw new Exception("No partial login");
            }

            var user = result.Identity;

            var claims_to_keep = new List<Claim>();
            if (user.HasClaim(c => c.Type == Constants.ClaimTypes.PartialLoginRestartUrl))
            {
                claims_to_keep.Add(user.FindFirst(Constants.ClaimTypes.PartialLoginRestartUrl));
            }
            if (user.HasClaim(c => c.Type == Constants.ClaimTypes.PartialLoginReturnUrl))
            {
                claims_to_keep.Add(user.FindFirst(Constants.ClaimTypes.PartialLoginReturnUrl));
            }
            if (user.HasClaim(c => c.Type == Constants.ClaimTypes.ExternalProviderUserId))
            {
                claims_to_keep.Add(user.FindFirst(Constants.ClaimTypes.ExternalProviderUserId));
            }
            if (user.HasClaim(c => c.Type.StartsWith(Constants.PartialLoginResumeClaimPrefix)))
            {
                claims_to_keep.Add(user.FindFirst(c => c.Type.StartsWith(Constants.PartialLoginResumeClaimPrefix)));
            }

            claims = claims.Where(x =>
                x.Type != Constants.ClaimTypes.PartialLoginRestartUrl &&
                x.Type != Constants.ClaimTypes.PartialLoginReturnUrl &&
                x.Type != Constants.ClaimTypes.ExternalProviderUserId &&
                !x.Type.StartsWith(Constants.PartialLoginResumeClaimPrefix));
            
            claims_to_keep.AddRange(claims);

            var new_user = new ClaimsIdentity(user.AuthenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
            new_user.AddClaims(claims_to_keep);

            context.Authentication.SignIn(result.Properties, new_user);
        }

        /// <summary>
        /// Updates the partial login with the authentication values provided.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="name">The name.</param>
        /// <param name="claims">The claims.</param>
        /// <param name="identityProvider">The identity provider.</param>
        /// <param name="authenticationMethod">The authentication method.</param>
        /// <returns></returns>
        public static async Task UpdatePartialLoginClaimsAsync(
            this IDictionary<string, object> env,
            string subject, string name,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider,
            string authenticationMethod = null
        )
        {
            if (env == null) throw new ArgumentNullException("env");

            var authResult = new IdentityServer3.Core.Models.AuthenticateResult(subject, name, claims, identityProvider, authenticationMethod);
            await env.UpdatePartialLoginClaimsAsync(authResult.User.Claims);
        }

        /// <summary>
        /// Gets the URL to restart the login process from the partial login.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">env</exception>
        /// <exception cref="System.Exception">No partial login</exception>
        public static async Task<string> GetPartialLoginRestartUrlAsync(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            var context = new OwinContext(env);
            var result = await context.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (result == null || result.Identity == null || result.Identity.IsAuthenticated == false)
            {
                throw new Exception("No partial login");
            }

            return result.Identity.Claims.Where(x => x.Type == Constants.ClaimTypes.PartialLoginRestartUrl)
                .Select(x => x.Value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the partial login resume URL.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">env</exception>
        /// <exception cref="System.Exception">No partial login</exception>
        public static async Task<string> GetPartialLoginResumeUrlAsync(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");
            
            var context = new OwinContext(env);
            var result = await context.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (result == null || result.Identity == null || result.Identity.IsAuthenticated == false)
            {
                throw new Exception("No partial login");
            }

            return result.Identity.Claims.Where(x => x.Type == Constants.ClaimTypes.PartialLoginReturnUrl)
                .Select(x => x.Value)
                .FirstOrDefault();
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
        /// Gets the sign in message.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// env
        /// or
        /// id
        /// </exception>
        public static SignInMessage GetSignInMessage(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            var ctx = new OwinContext(env);
            var id = ctx.Request.Query.Get(Constants.Authentication.SigninId);

            if (String.IsNullOrWhiteSpace(id)) return null;

            return env.GetSignInMessage(id);
        }

        /// <summary>
        /// Gets the current fully logged in IdentityServer user. Returns null if the user is not logged in.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">env</exception>
        public static async Task<ClaimsIdentity> GetIdentityServerFullLoginAsync(this IDictionary<string, object> env)
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
        public static async Task<ClaimsIdentity> GetIdentityServerPartialLoginAsync(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            var context = new OwinContext(env);
            return await context.GetIdentityFrom(Constants.PartialSignInAuthenticationType);
        }

        /// <summary>
        /// Removes the partial login cookie.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <exception cref="System.ArgumentNullException">env</exception>
        public static void RemovePartialLoginCookie(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            var context = new OwinContext(env);
            context.Authentication.SignOut(Constants.PartialSignInAuthenticationType);
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