using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
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
            var idToken = _tokenService.CreateIdentityToken(request, user);

            SigningCredentials credentials;
            if (request.Client.IdentityTokenSigningKeyType == SigningKeyTypes.ClientSecret)
            {
                credentials = new HmacSigningCredentials(request.Client.ClientSecret);
            }
            else
            {
                credentials = new X509SigningCredentials(_settings.GetSigningCertificate());
            }

            var jwt = _tokenService.CreateJsonWebToken(idToken, credentials);

            string accessTokenReference = null;
            int accessTokenLifetime = 0;
            
            if (request.AccessTokenRequested)
            {
                var accessToken = _tokenService.CreateAccessToken(request, user);
                accessTokenLifetime = accessToken.Lifetime;

                accessTokenReference = Guid.NewGuid().ToString("N");
                _tokenHandles.Store(accessTokenReference, accessToken);
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
