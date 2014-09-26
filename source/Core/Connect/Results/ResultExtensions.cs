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

using System;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Results;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Results;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public static class ResultExtensions
    {
        public static IHttpActionResult AuthorizeCodeResponse(this ApiController controller, Uri redirectUri, string code, string state)
        {
            return new AuthorizeCodeResult(new AuthorizeResponse
                {
                    RedirectUri = redirectUri,
                    Code = code,
                    State = state
                });
        }

        public static IHttpActionResult AuthorizeCodeResponse(this ApiController controller, AuthorizeResponse response)
        {
            return new AuthorizeCodeResult(response);
        }

        public static IHttpActionResult AuthorizeImplicitFragmentResponse(this ApiController controller, Uri redirectUri, string identityToken, string accessToken, int accessTokenLifetime, string state)
        {
            return new AuthorizeImplicitFragmentResult(new AuthorizeResponse
            {
                RedirectUri = redirectUri,
                IdentityToken = identityToken,
                AccessToken = accessToken,
                AccessTokenLifetime = accessTokenLifetime,
                State = state
            });
        }

        public static IHttpActionResult AuthorizeImplicitFragmentResponse(this ApiController controller, AuthorizeResponse response)
        {
            return new AuthorizeImplicitFragmentResult(response);
        }

        public static IHttpActionResult AuthorizeImplicitFormPostResponse(this ApiController controller, Uri redirectUri, string identityToken, string state)
        {
            return new AuthorizeImplicitFormPostResult(controller.Request, new AuthorizeResponse
            {
                RedirectUri = redirectUri,
                IdentityToken = identityToken,
                State = state
            });
        }

        public static IHttpActionResult AuthorizeImplicitFormPostResponse(this ApiController controller, AuthorizeResponse response)
        {
            return new AuthorizeImplicitFormPostResult(controller.Request, response);
        }

        public static IHttpActionResult TokenResponse(this ApiController controller, TokenResponse response)
        {
            return new TokenResult(response);
        }

        public static IHttpActionResult TokenErrorResponse(this ApiController controller, string error)
        {
            return new TokenErrorResult(error);
        }
    }
}