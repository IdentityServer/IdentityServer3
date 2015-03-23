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

using System.Net.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services.DefaultViewService;

namespace Thinktecture.IdentityServer.Core.Results
{
    internal class CheckSessionResult : HtmlActionResult
    {
        private readonly IdentityServerOptions _options;
        private readonly HttpRequestMessage _request;

        public CheckSessionResult(IdentityServerOptions options, HttpRequestMessage request)
        {
            _options = options;
            _request = request;
        }

        protected override string GetHtml()
        {
            var root = _request.GetIdentityServerBaseUrl();
            if (root.EndsWith("/")) root = root.Substring(0, root.Length - 1);

            return AssetManager.LoadCheckSession(root, _options.AuthenticationOptions.CookieOptions.GetSessionCookieName());
        }
    }
}