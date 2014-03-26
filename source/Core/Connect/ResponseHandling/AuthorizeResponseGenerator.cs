using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class AuthorizeResponseGenerator
    {
        private ITokenService _tokenService;
        private IAuthorizationCodeStore _authorizationCodes;
        private ITokenHandleStore _tokenHandles;
        private ICoreSettings _settings;

        public AuthorizeResponseGenerator(ITokenService tokenService, IAuthorizationCodeStore authorizationCodes, ITokenHandleStore tokenHandles, ICoreSettings settings)
        {
            _tokenService = tokenService;
            _authorizationCodes = authorizationCodes;
            _tokenHandles = tokenHandles;
            _settings = settings;
        }

        public AuthorizeResponse CreateCodeFlowResponse(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            var code = new AuthorizationCode
            {
                Client = request.Client,
                User = user,

                IsOpenId = request.IsOpenIdRequest,
                RequestedScopes = request.ValidatedScopes.GrantedScopes,
                RedirectUri = request.RedirectUri,
            };

            // store id token and access token and return authorization code
            var id = Guid.NewGuid().ToString("N");
            _authorizationCodes.Store(id, code);

            return new AuthorizeResponse
            {
                RedirectUri = request.RedirectUri,
                Code = id,
                State = request.State
            };
        }

        public AuthorizeResponse CreateImplicitFlowResponse(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            string accessTokenValue = null;
            int accessTokenLifetime = 0;
            if (request.IsResourceRequest)
            {
                var accessToken = _tokenService.CreateAccessToken(user, request.Client, request.ValidatedScopes.GrantedScopes, request.Raw);
                accessTokenLifetime = accessToken.Lifetime;

                accessTokenValue = _tokenService.CreateSecurityToken(accessToken);
            }

            string jwt = null;
            if (request.IsOpenIdRequest)
            {
                var idToken = _tokenService.CreateIdentityToken(user, request.Client, request.ValidatedScopes.GrantedScopes, !request.AccessTokenRequested, request.Raw, accessTokenValue);
                jwt = _tokenService.CreateSecurityToken(idToken);
            }

            return new AuthorizeResponse
            {
                RedirectUri = request.RedirectUri,
                AccessToken = accessTokenValue,
                AccessTokenLifetime = accessTokenLifetime,
                IdentityToken = jwt,
                State = request.State,
                Scope = request.ValidatedScopes.GrantedScopes.ToSpaceSeparatedString()
            };
        }
    }
}
