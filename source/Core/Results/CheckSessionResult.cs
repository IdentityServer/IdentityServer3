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
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Services.Default;
using System.Net.Http;

namespace IdentityServer3.Core.Results
{
    internal class CheckSessionResult : HtmlActionResult
    {
        private readonly IdentityServerOptions options;
        private readonly HttpRequestMessage request;

        public CheckSessionResult(IdentityServerOptions options, HttpRequestMessage request)
        {
            this.options = options;
            this.request = request;
        }

        protected override string GetHtml()
        {
            var root = request.GetIdentityServerBaseUrl();
            if (root.EndsWith("/")) root = root.Substring(0, root.Length - 1);

            return AssetManager.LoadCheckSession(root, options.AuthenticationOptions.CookieOptions.GetSessionCookieName());
        }
    }
}