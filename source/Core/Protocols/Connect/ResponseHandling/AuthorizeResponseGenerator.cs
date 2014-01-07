using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect
{
    public class OidcAuthorizeResponseGenerator
    {
        private IOidcTokenService _tokenService;
        private IAuthorizationCodeService _authorizationCodes;
        private ITokenHandleService _tokenHandles;

        public OidcAuthorizeResponseGenerator(IOidcTokenService tokenService, IAuthorizationCodeService authorizationCodes, ITokenHandleService tokenHandles)
        {
            _tokenService = tokenService;
            _authorizationCodes = authorizationCodes;
            _tokenHandles = tokenHandles;
        }

        public AuthorizeResponse CreateCodeFlowResponse(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            // create id and access token
            var idToken = _tokenService.CreateIdentityToken(request, user);
            var accessToken = _tokenService.CreateAccessToken(request, user);

            var code = new AuthorizationCode
            {
                ClientId = request.ClientId,
                IsOpenId = true,
                RequestedScopes = request.Scopes.ToSpaceSeparatedString(),

                CreationTime = DateTime.UtcNow,
                RedirectUri = request.RedirectUri,

                IdentityToken = idToken,
                AccessToken = accessToken
            };

            // store id token and access token and return authorization code
            var id = _authorizationCodes.Store(code);

            return new AuthorizeResponse
            {
                RedirectUri = request.RedirectUri,
                Code = id,
                State = request.State
            };
        }

        public AuthorizeResponse CreateImplicitFlowResponse(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            var idToken = _tokenService.CreateIdentityToken(request, user);
            var jwt = _tokenService.CreateJsonWebToken(idToken, request.Client, request.Configuration);

            string accessTokenReference = null;
            int accessTokenLifetime = 0;
            
            if (request.AccessTokenRequested)
            {
                var accessToken = _tokenService.CreateAccessToken(request, user);
                accessTokenLifetime = accessToken.Lifetime;
                accessTokenReference = _tokenHandles.Store(accessToken);
            }

            return new AuthorizeResponse
            {
                RedirectUri = request.RedirectUri,
                AccessToken = accessTokenReference,
                AccessTokenLifetime = accessTokenLifetime,
                IdentityToken = jwt,
                State = request.State
            };
        }
    }
}
