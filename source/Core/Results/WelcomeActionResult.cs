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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Services.Default;
using Microsoft.Owin;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Results
{
    internal class WelcomeActionResult : IHttpActionResult
    {
        readonly IOwinContext context;

        public WelcomeActionResult(IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;
        }

        public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            var baseUrl = context.GetIdentityServerBaseUrl();
            
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            var html = AssetManager.LoadWelcomePage(baseUrl, version);
            var content = new StringContent(html, Encoding.UTF8, "text/html");

            var response = new HttpResponseMessage
            {
                Content = content
            };

            return Task.FromResult(response);
        }
    }
}