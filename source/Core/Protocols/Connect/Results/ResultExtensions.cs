using System;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Results;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect
{
    public static class ResultExtensions
    {
        public static IHttpActionResult AuthorizeError(this ApiController controller, ErrorTypes errorType, string error, string responseMode, Uri errorUri, string state)
        {
            return new OidcAuthorizeErrorResult(new AuthorizeError
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
            return new OidcAuthorizeCodeResult(new AuthorizeResponse
                {
                    RedirectUri = redirectUri,
                    Code = code,
                    State = state
                });
        }

        public static IHttpActionResult AuthorizeCodeResponse(this ApiController controller, AuthorizeResponse response)
        {
            return new OidcAuthorizeCodeResult(response);
        }

        public static IHttpActionResult AuthorizeImplicitFragmentResponse(this ApiController controller, Uri redirectUri, string identityToken, string accessToken, int accessTokenLifetime, string state)
        {
            return new OidcAuthorizeImplicitFragmentResult(new AuthorizeResponse
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
            return new OidcAuthorizeImplicitFragmentResult(response);
        }

        public static IHttpActionResult AuthorizeImplicitFormPostResponse(this ApiController controller, Uri redirectUri, string identityToken, string state)
        {
            return new OidcAuthorizeImplicitFormPostResult(new AuthorizeResponse
            {
                RedirectUri = redirectUri,
                IdentityToken = identityToken,
                State = state
            });
        }

        public static IHttpActionResult AuthorizeImplicitFormPostResponse(this ApiController controller, AuthorizeResponse response)
        {
            return new OidcAuthorizeImplicitFormPostResult(response);
        }

        public static IHttpActionResult Login(this ApiController controller, SignInMessage message)
        {
            return new LoginResult(message, controller.Request);
        }

        public static IHttpActionResult TokenResponse(this ApiController controller, TokenResponse response)
        {
            return new OidcTokenResult(response);
        }

        public static IHttpActionResult TokenErrorResponse(this ApiController controller, string error)
        {
            return new OidcTokenErrorResult(error);
        }
    }
}
