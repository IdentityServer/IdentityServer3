using System;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenResponseGenerator
    {
        private ICoreSettings _coreSettings;
        private ITokenService _tokenService;
        private ITokenHandleStore _tokenHandles;

        public TokenResponseGenerator(ITokenService tokenService, ITokenHandleStore tokenHandles, ICoreSettings coreSettings, IAuthorizationCodeStore codes)
        {
            _coreSettings = coreSettings;
            _tokenService = tokenService;
            _tokenHandles = tokenHandles;
        }

        public TokenResponse Process(ValidatedTokenRequest request, ClaimsPrincipal client)
        {
            var idToken = _tokenService.CreateIdentityToken(request, client);
            var accessToken = _tokenService.CreateAccessToken(request, client);
            var jwt = _tokenService.CreateJsonWebToken(idToken, request.Client, _coreSettings);

            var accessTokenReference = Guid.NewGuid().ToString("N");
            _tokenHandles.Store(accessTokenReference, accessToken);

            return new TokenResponse
            {
                Jwt = jwt,
                AccessTokenReference = accessTokenReference,
                AccessTokenLifetime = accessToken.Lifetime
            };
        }
    }
}
