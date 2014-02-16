using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenResponseGenerator
    {
        private ICoreSettings _settings;
        private ITokenService _tokenService;
        private ITokenHandleStore _tokenHandles;

        public TokenResponseGenerator(ITokenService tokenService, ITokenHandleStore tokenHandles, ICoreSettings settings, IAuthorizationCodeStore codes)
        {
            _settings = settings;
            _tokenService = tokenService;
            _tokenHandles = tokenHandles;
        }

        public TokenResponse Process(ValidatedTokenRequest request, ClaimsPrincipal client)
        {
            var idToken = _tokenService.CreateIdentityToken(request, client);
            var accessToken = _tokenService.CreateAccessToken(request, client);

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
