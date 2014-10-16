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
using System;
using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    public static class OwinExtensions
    {
        public static string GetBaseUrl(this IDictionary<string, object> env, string host = null)
        {
            var ctx = new OwinContext(env);
            var request = ctx.Request;
            
            if (host.IsMissing())
            {
                host = request.Uri.Scheme + "://" + request.Host.Value;
            }

            var baseUrl = new Uri(new Uri(host), ctx.Request.PathBase.Value).AbsoluteUri;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            return baseUrl;
        }

        public static string GetIdentityServerLogoutUrl(this IDictionary<string, object> env)
        {
            return env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Logout;
        }
        
        public static string GetIdentityServerBaseUrl(this IDictionary<string, object> env)
        {
            return env[Constants.OwinEnvironment.IdentityServerBaseUrl] as string;
        }

        public static void SetIdentityServerBaseUrl(this IDictionary<string, object> env, string value)
        {
            env[Constants.OwinEnvironment.IdentityServerBaseUrl] = value;
        }

        public static ILifetimeScope GetLifetimeScope(this IDictionary<string, object> env)
        {
            return new OwinContext(env).Get<ILifetimeScope>(Constants.OwinEnvironment.AutofacScope);
        }
        public static void SetLifetimeScope(this IDictionary<string, object> env, ILifetimeScope scope)
        {
            new OwinContext(env).Set<ILifetimeScope>(Constants.OwinEnvironment.AutofacScope, scope);
        }
    }
}