﻿/*
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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class LoginResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignInMessage message;
        private readonly IDictionary<string, object> env;
        private readonly IdentityServerOptions options;

        public static string GetRedirectUrl(SignInMessage message, IDictionary<string, object> env, IdentityServerOptions options)
        {
            var result = new LoginResult(message, env, options);
            var response = result.Execute();

            return response.Headers.Location.AbsoluteUri;
        }
        
        public LoginResult(SignInMessage message, IDictionary<string, object> env, IdentityServerOptions options)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (env == null) throw new ArgumentNullException("env");
            if (options == null) throw new ArgumentNullException("options");

            this.message = message;
            this.env = env;
            this.options = options;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            Logger.Info("Redirecting to login page");

            var cookie = new MessageCookie<SignInMessage>(this.env, this.options);
            var id = cookie.Write(this.message);

            var url = env.GetIdentityServerBaseUrl() + Constants.RoutePaths.Login;
            var uri = new Uri(url.AddQueryString("signin=" + id));

            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = uri;
            return response;
        }
    }
}