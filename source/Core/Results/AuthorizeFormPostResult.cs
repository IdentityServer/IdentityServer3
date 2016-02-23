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
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.ViewModels;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Results
{
    internal class AuthorizeFormPostResult : HtmlActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly AuthorizeResponse _response;
        private readonly HttpRequestMessage _request;

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
            var redirect = _response.RedirectUri;

            return AssetManager.LoadFormPost(root, redirect, fields);
        }

        public override async Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            _request.SetSuppressXfo();

            Logger.Info("Posting to " + _response.RedirectUri);

            // see if we have a DefaultViewService for the IViewService 
            // to allow for customization of the authorize response page
            var ctx = _request.GetOwinContext();
            var defaultViewSvc = ctx.ResolveDependency<IViewService>() as DefaultViewService;
            if (defaultViewSvc != null)
            {
                Logger.Debug("Using DefaultViewService to render authorization response HTML");

                var vm = new AuthorizeResponseViewModel
                {
                    SiteName = _response.Request.Options.SiteName,
                    SiteUrl = _request.GetIdentityServerBaseUrl(),
                    ResponseFormUri = _response.RedirectUri,
                    ResponseFormFields = _response.ToNameValueCollection().ToFormPost()
                };

                var result = new HtmlStreamActionResult(() => defaultViewSvc.AuthorizeResponse(vm));
                return await result.ExecuteAsync(cancellationToken);
            }

            Logger.Debug("Using AssetManager to render authorization response HTML");
            return await base.ExecuteAsync(cancellationToken);
        }
    }
}