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

using Autofac;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Models;
using System.Threading.Tasks;
using System.Security.Claims;

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
            return new OwinContext(env).Get<ILifetimeScope>(Constants.OwinEnvironment.AutofacScope);
        }

        internal static void SetLifetimeScope(this IDictionary<string, object> env, ILifetimeScope scope)
        {
            new OwinContext(env).Set<ILifetimeScope>(Constants.OwinEnvironment.AutofacScope, scope);
        }

        internal static T ResolveDependency<T>(this IDictionary<string, object> env)
        {
            var scope = env.GetLifetimeScope();
            var instance = (T)scope.ResolveOptional(typeof(T));
            return instance;
        }

        internal static IEnumerable<AuthenticationDescription> GetExternalAuthenticationTypes(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Authentication.GetAuthenticationTypes(d => d.Caption.IsPresent());
        }
        
        internal static IEnumerable<AuthenticationDescription> GetExternalAuthenticationTypes(this IOwinContext context, IEnumerable<string> typeFilter)
        {
            var types = context.GetExternalAuthenticationTypes();
            
            if (typeFilter != null && typeFilter.Any())
            {
                types = types.Where(type => typeFilter.Contains(type.AuthenticationType));
            }
            
            return types;
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
                string val = null;
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

        internal static string CreateSignInRequest(this IOwinContext context, SignInMessage message)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Environment.CreateSignInRequest(message);
        }
    }
}