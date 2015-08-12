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
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Extensions
{
    internal static class InternalOwinExtensions
    {
        public static string GetRequestId(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Environment.GetRequestId();
        }

        public static void SetRequestId(this IDictionary<string, object> env, string id)
        {
            if (env == null) throw new ArgumentNullException("env");

            env[Constants.OwinEnvironment.RequestId] = id;
        }

        public static void SetIdentityServerHost(this IDictionary<string, object> env, string value)
        {
            if (env == null) throw new ArgumentNullException("env");

            env[Constants.OwinEnvironment.IdentityServerHost] = value;
        }

        public static void SetIdentityServerBasePath(this IDictionary<string, object> env, string value)
        {
            if (env == null) throw new ArgumentNullException("env");

            env[Constants.OwinEnvironment.IdentityServerBasePath] = value;
        }

        public static ILifetimeScope GetLifetimeScope(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            return new OwinContext(env).GetAutofacLifetimeScope();
        }

        public static T ResolveDependency<T>(this IDictionary<string, object> env)
        {
            if (env == null) throw new ArgumentNullException("env");

            var scope = env.GetLifetimeScope();
            var instance = (T)scope.ResolveOptional(typeof(T));
            return instance;
        }

        public static T ResolveDependency<T>(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            
            return context.Environment.ResolveDependency<T>();
        }

        public static IEnumerable<AuthenticationDescription> GetExternalAuthenticationProviders(this IOwinContext context, IEnumerable<string> filter = null)
        {
            if (context == null) throw new ArgumentNullException("context");

            var types = context.Authentication.GetAuthenticationTypes().Where(x => !Constants.IdentityServerAuthenticationTypes.Contains(x.AuthenticationType));

            if (filter != null && filter.Any())
            {
                types = types.Where(x => filter.Contains(x.AuthenticationType));
            }

            return types;
        }

        public static bool IsValidExternalAuthenticationProvider(this IOwinContext context, string name)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Authentication.GetAuthenticationTypes().Any(x => x.AuthenticationType == name);
        }

        public static IEnumerable<LoginPageLink> GetLinksFromProviders(this IOwinContext context, IEnumerable<AuthenticationDescription> types, string signInMessageId)
        {
            if (context == null) throw new ArgumentNullException("context");

            if (types != null)
            {
                return types.Select(p => new LoginPageLink
                {
                    Type = p.AuthenticationType,
                    Text = p.Caption,
                    Href = context.GetExternalProviderLoginUrl(p.AuthenticationType, signInMessageId)
                });
            }

            return Enumerable.Empty<LoginPageLink>();
        }

        public static IEnumerable<LoginPageLink> FilterHiddenLinks(this IEnumerable<LoginPageLink> links)
        {
            if (links == null) throw new ArgumentNullException("links");

            return links.Where(x => x.Text.IsPresent());
        }

        public static async Task<Microsoft.Owin.Security.AuthenticateResult> GetAuthenticationFrom(this IOwinContext context, string authenticationType)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (authenticationType.IsMissing()) throw new ArgumentNullException("authenticationType");

            return await context.Authentication.AuthenticateAsync(authenticationType);
        }

        public static async Task<ClaimsIdentity> GetIdentityFrom(this IOwinContext context, string authenticationType)
        {
            if (context == null) throw new ArgumentNullException("context");
            
            var result = await context.GetAuthenticationFrom(authenticationType);
            if (result != null &&
                result.Identity != null &&
                result.Identity.IsAuthenticated)
            {
                return result.Identity;
            }
            return null;
        }

        public static async Task<ClaimsIdentity> GetIdentityFromPartialSignIn(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return await context.GetIdentityFrom(Constants.PartialSignInAuthenticationType);
        }

        public static async Task<ClaimsIdentity> GetIdentityFromExternalSignIn(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return await context.GetIdentityFrom(Constants.ExternalAuthenticationType);
        }

        public static async Task<string> GetSignInIdFromExternalProvider(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            
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

        public static async Task<ClaimsIdentity> GetIdentityFromExternalProvider(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

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

        public static string GetCspReportUrl(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.CspReport;
        }

        public static string GetPartialLoginRestartUrl(this IOwinContext context, string signinId)
        {
            if (context == null) throw new ArgumentNullException("context");

            if (String.IsNullOrWhiteSpace(signinId)) throw new ArgumentNullException("signinId");
            
            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.Login + "?signin=" + signinId;
        }
        
        public static string GetPartialLoginResumeUrl(this IOwinContext context, string resumeId)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (String.IsNullOrWhiteSpace(resumeId)) throw new ArgumentNullException("resumeId");
            
            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.ResumeLoginFromRedirect + "?resume=" + resumeId;
        }

        public static string GetPermissionsPageUrl(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.ClientPermissions;
        }

        public static string GetExternalProviderLoginUrl(this IOwinContext context, string provider, string signinId)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(signinId)) throw new ArgumentNullException("signinId");

            return context.Environment.GetIdentityServerBaseUrl() + Constants.RoutePaths.LoginExternal + "?provider=" + provider + "&signin=" + signinId;
        }

        public static string GetIdentityServerHost(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Environment.GetIdentityServerHost();
        }

        public static string GetIdentityServerBasePath(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Environment.GetIdentityServerBasePath();
        }

        public static string GetIdentityServerBaseUrl(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Environment.GetIdentityServerBaseUrl();
        }

        public static string GetIdentityServerLogoutUrl(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Environment.GetIdentityServerLogoutUrl();
        }

        public static string GetCurrentUserDisplayName(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            if (context.Authentication.User != null &&
                context.Authentication.User.Identity != null)
            {
                return context.Authentication.User.Identity.Name;
            }

            return null;
        }

        public static string CreateSignInRequest(this IOwinContext context, SignInMessage message)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Environment.CreateSignInRequest(message);
        }

        public async static Task<IFormCollection> ReadRequestFormAsync(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            // hack to clear a possible cached type from Katana in environment
            context.Environment.Remove("Microsoft.Owin.Form#collection");

            if (!context.Request.Body.CanSeek)
            {
                var copy = new MemoryStream();
                await context.Request.Body.CopyToAsync(copy);
                copy.Seek(0L, SeekOrigin.Begin);
                context.Request.Body = copy;
            }

            context.Request.Body.Seek(0L, SeekOrigin.Begin);
            var form = await context.Request.ReadFormAsync();
            context.Request.Body.Seek(0L, SeekOrigin.Begin);

            // hack to prevent caching of an internalized type from Katana in environment
            context.Environment.Remove("Microsoft.Owin.Form#collection");

            return form;
        }

        public async static Task<string> ReadBodyAsStringAsync(this IOwinRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            if (!request.Body.CanSeek)
            {
                var copy = new MemoryStream();
                await request.Body.CopyToAsync(copy);
                copy.Seek(0L, SeekOrigin.Begin);
                request.Body = copy;
            }

            request.Body.Seek(0L, SeekOrigin.Begin);
            
            string body = null;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 4096, true))
            {
                body = await reader.ReadToEndAsync();
            }

            request.Body.Seek(0L, SeekOrigin.Begin);
            
            return body;
        }

        public async static Task<NameValueCollection> ReadRequestFormAsNameValueCollectionAsync(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            
            var form = await context.ReadRequestFormAsync();

            NameValueCollection nv = new NameValueCollection();

            foreach (var item in form)
            {
                if (item.Value.Any())
                {
                    nv.Add(item.Key, item.Value[0]);
                }
            }

            return nv;
        }
    }
}