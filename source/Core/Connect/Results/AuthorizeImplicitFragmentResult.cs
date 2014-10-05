///*
// * Copyright 2014 Dominick Baier, Brock Allen
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// *   http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */

//using System;
//using System.Collections.Specialized;
//using System.Net;
//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Web.Http;
//using Thinktecture.IdentityServer.Core.Connect.Models;
//using Thinktecture.IdentityServer.Core.Extensions;
//using Thinktecture.IdentityServer.Core.Logging;

//namespace Thinktecture.IdentityServer.Core.Connect.Results
//{
//    public class AuthorizeImplicitFragmentResult : IHttpActionResult
//    {
//        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
//        private readonly AuthorizeResponse _response;

//        public AuthorizeImplicitFragmentResult(AuthorizeResponse response)
//        {
//            _response = response;
//        }

//        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
//        {
//            return Task.FromResult(Execute());
//        }

//        private HttpResponseMessage Execute()
//        {
//            var responseMessage = new HttpResponseMessage(HttpStatusCode.Redirect);
//            var url = _response.RedirectUri.AbsoluteUri;
//            var query = new NameValueCollection();

//            if (_response.IdentityToken.IsPresent())
//            {
//                query.Add("id_token", _response.IdentityToken);
//            }
            
//            if (_response.AccessToken.IsPresent())
//            {
//                query.Add("access_token", _response.AccessToken);
//                query.Add("token_type", "Bearer");
//                query.Add("expires_in", _response.AccessTokenLifetime.ToString());
//            }

//            if (_response.Scope.IsPresent())
//            {
//                query.Add("scope", _response.Scope);
//            }

//            if (_response.State.IsPresent())
//            {
//                query.Add("state", _response.State);
//            }

//            url = string.Format("{0}#{1}", url, query.ToQueryString());
//            responseMessage.Headers.Location = new Uri(url);
            
//            Logger.Info("Redirecting to " + _response.RedirectUri.AbsoluteUri);
            
//            return responseMessage;
//        }
//    }
//}