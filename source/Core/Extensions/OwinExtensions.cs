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

using Autofac;
using Autofac.Integration.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    /// <summary>
    /// Extension methods for the OWIN environment.
    /// </summary>
    public static class OwinExtensions
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

        internal static string GetRequestId(this IOwinContext ctx)
        {
            return ctx.Environment.GetRequestId();
        }

        internal static void SetRequestId(this IDictionary<string, object> env, string id)
        {
            env[Constants.OwinEnvironment.RequestId] = id;
        }


        internal static void SetIdentityServerHost(this IDictionary<string, object> env, string value)
        {
            env[Constants.OwinEnvironment.IdentityServerHost] = value;
        }
        
        internal static void SetIdentityServerBasePath(this IDictionary<string, object> env, string value)
        {
            env[Constants.OwinEnvironment.IdentityServerBasePath] = value;
        }


        internal static ILifetimeScope GetLifetimeScope(this IDictionary<string, object> env)
        {
            return new OwinContext(env).GetAutofacLifetimeScope();
        }

        internal static T ResolveDependency<T>(this IDictionary<string, object> env)
        {
            var scope = env.GetLifetimeScope();
            var instance = (T)scope.ResolveOptional(typeof(T));
            return instance;
        }

        internal static T ResolveDependency<T>(this IOwinContext context)
        {
            return context.Environment.ResolveDependency<T>();
        }

        internal static IEnumerable<AuthenticationDescription> GetExternalAuthenticationProviders(this IOwinContext context, IEnumerable<string> filter = null)
        {
            if (context == null) throw new ArgumentNullException("context");

            var types = context.Authentication.GetAuthenticationTypes().Where(x => !Constants.IdentityServerAuthenticationTypes.Contains(x.AuthenticationType));
            
            if (filter != null && filter.Any())
            {
                types = types.Where(x=>filter.Contains(x.AuthenticationType));
            }
            
            return types;
        }

        internal static bool IsValidExternalAuthenticationProvider(this IOwinContext context, string name)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Authentication.GetAuthenticationTypes().Any(x => x.AuthenticationType == name);
        }

        internal static IEnumerable<LoginPageLink> GetLinksFromProviders(this IOwinContext context, IEnumerable<AuthenticationDescription> types, string signInMessageId)
        {
            if (context == null) throw new ArgumentNullException("context");

            if (types != null)
            {
                return types.Select(p => new LoginPageLink
                {
                    Text = p.Caption,
                    Href = context.GetExternalProviderLoginUrl(p.AuthenticationType, signInMessageId)
                });
            }
            
            return Enumerable.Empty<LoginPageLink>();
        }

        internal static IEnumerable<LoginPageLink> FilterHiddenLinks(this IEnumerable<LoginPageLink> links)
        {
            if (links == null) throw new ArgumentNullException("links");
            return links.Where(x=>x.Text.IsPresent());
        }

        static async Task<Microsoft.Owin.Security.AuthenticateResult> GetAuthenticationFrom(this IOwinContext context, string authenticationType)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (authenticationType.IsMissing()) throw new ArgumentNullException("authenticationType");
            
            return await context.Authentication.AuthenticateAsync(authenticationType);
        }

        static async Task<ClaimsIdentity> GetIdentityFrom(this IOwinContext context, string authenticationType)
        {
            var result = await context.GetAuthenticationFrom(authenticationType);
            if (result != null &&
                result.Identity != null &&
                result.Identity.IsAuthenticated)
            {
                return result.Identity;
            }
            return null;
        }

        internal static async Task<ClaimsIdentity> GetIdentityFromPartialSignIn(this IOwinContext context)
        {
            return await context.GetIdentityFrom(Constants.PartialSignInAuthenticationType);
        }

        internal static async Task<ClaimsIdentity> GetIdentityFromExternalSignIn(this IOwinContext context)
        {
            return await context.GetIdentityFrom(Constants.ExternalAuthenticationType);
        }

        internal static async Task<string> GetSignInIdFromExternalProvider(this IOwinContext context)
        {
            var result = await context.GetAuthenticationFrom(Constants.ExternalAuthenticationType);
            if (result != null)
            {
                string val;
                if (result.Properties.Dictionary.TryGetValue(Constants.Authentication.SigninId, out val))
                {
                    return val;
                }
            }
            return null;
        }

        internal static async Task<ClaimsIdentity> GetIdentityFromExternalProvider(this IOwinContext context)
        {
            var id = await context.GetIdentityFromExternalSignIn();
            if (id != null)
            {
                // this is mapping from the external IdP's issuer to the name of the 
                // katana middleware that's registered in startup
                var result = await context.GetAuthenticationFrom(Constants.ExternalAuthenticationType);
                if (!result.Properties.Dictionary.Keys.Contains(Constants.Authentication.KatanaAuthenticationType))
                {
                    throw new InvalidOperationException("Missing KatanaAuthenticationType");
                }

                var provider = result.Properties.Dictionary[Constants.Authentication.KatanaAuthenticationType];
                var newClaims = id.Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, provider));
                id = new ClaimsIdentity(newClaims, id.AuthenticationType);
            }
            return id;
        }

        internal static string GetCspReportUrl(this IOwinContext context)
        {
            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.CspReport;
        }

        internal static string GetPartialLoginResumeUrl(this IOwinContext context, string resumeId)
        {
            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.ResumeLoginFromRedirect + "?resume=" + resumeId;
        }
        
        internal static string GetPermissionsPageUrl(this IOwinContext context)
        {
            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.ClientPermissions;
        }

        internal static string GetExternalProviderLoginUrl(this IOwinContext context, string provider, string signinId)
        {
            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.LoginExternal + "?provider=" + provider + "&signin=" + signinId;
        }


        internal static string GetIdentityServerHost(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Environment.GetIdentityServerHost();
        }

        internal static string GetIdentityServerBasePath(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Environment.GetIdentityServerBasePath();
        }

        internal static string GetIdentityServerBaseUrl(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Environment.GetIdentityServerBaseUrl();
        }

        internal static string GetIdentityServerLogoutUrl(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Environment.GetIdentityServerLogoutUrl();
        }
        
        internal static string GetCurrentUserDisplayName(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            
            if (context.Authentication.User != null && 
                context.Authentication.User.Identity != null)
            {
                return context.Authentication.User.Identity.Name;
            }
            
            return null;
        }

        internal static string CreateSignInRequest(this IOwinContext context, SignInMessage message)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Environment.CreateSignInRequest(message);
        }
    }
}