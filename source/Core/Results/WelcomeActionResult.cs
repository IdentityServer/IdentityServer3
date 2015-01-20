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

using Microsoft.Owin;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.Default;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Core.Results
{
    internal class WelcomeActionResult : IHttpActionResult
    {
        IOwinContext context;
        public WelcomeActionResult(IOwinContext context)
        {
            this.context = context;
        }

        public Task<System.Net.Http.HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            var baseUrl = context.GetIdentityServerBaseUrl();
            var build = typeof(WelcomeActionResult).Assembly.GetName().Version.ToString();
            
            var html = AssetManager.LoadWelcomePage(baseUrl, build);
            var content = new StringContent(html, Encoding.UTF8, "text/html");

            var response = new HttpResponseMessage
            {
                Content = content
            };

            return Task.FromResult(response);
        }
    }
}