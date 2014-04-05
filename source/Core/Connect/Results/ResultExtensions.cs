/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
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
        public static IHttpActionResult AuthorizeError(this ApiController controller, AuthorizeError error)
        {
            return new AuthorizeErrorResult(error);
        }

        public static IHttpActionResult AuthorizeError(this ApiController controller, ErrorTypes errorType, string error, string responseMode, Uri errorUri, string state)
        {
            return new AuthorizeErrorResult(new AuthorizeError
                {
                    ErrorType = errorType,
                    Error = error,
                    ResponseMode = responseMode,
                    ErrorUri = errorUri,
                    State = state
                });
        }

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
            return new AuthorizeImplicitFormPostResult(new AuthorizeResponse
            {
                RedirectUri = redirectUri,
                IdentityToken = identityToken,
                State = state
            });
        }

        public static IHttpActionResult AuthorizeImplicitFormPostResponse(this ApiController controller, AuthorizeResponse response)
        {
            return new AuthorizeImplicitFormPostResult(response);
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