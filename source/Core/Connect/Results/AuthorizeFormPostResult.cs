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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Views;
using Thinktecture.IdentityServer.Core.Views.Embedded.Assets;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class AuthorizeFormPostResult : HtmlActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private AuthorizeResponse _response;
        private HttpRequestMessage _request;

        public AuthorizeFormPostResult(AuthorizeResponse response, HttpRequestMessage request)
        {
            _response = response;
            _request = request;
        }

        protected override string GetHtml()
        {
            var root = _request.GetIdentityServerBaseUrl();
            if (root.EndsWith("/")) root = root.Substring(0, root.Length - 1);
            var fields = _response.ToNameValueCollection().ToFormPost();
            var redirect = _response.RedirectUri.AbsoluteUri;

            string html = AssetManager.LoadResourceString("Thinktecture.IdentityServer.Core.Views.Embedded.Assets.app.FormPostResponse.html",
                new
                {
                    rootUrl = root,
                    redirect_uri = redirect,
                    fields = fields
                });

            return html;
        }

        public override Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            Logger.Info("Posting to " + _response.RedirectUri.AbsoluteUri);
            return base.ExecuteAsync(cancellationToken);
        }
    }
}