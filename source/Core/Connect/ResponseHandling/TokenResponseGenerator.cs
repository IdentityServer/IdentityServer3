using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenResponseGenerator
    {
        private ITokenService _tokenService;
        private ITokenHandleService _tokenHandles;
        private Configuration _configuration;
        
        public TokenResponseGenerator(ITokenService tokenService, ITokenHandleService tokenHandles, Configuration configuration)
        {
            _tokenService = tokenService;
            _tokenHandles = tokenHandles;
            _configuration = configuration;
        }

        public TokenResponse Process(ValidatedTokenRequest request, ClaimsPrincipal client)
        {
            var idToken = _tokenService.CreateIdentityToken(request, client);
            var accessToken = _tokenService.CreateAccessToken(request, client);
            var jwt = _tokenService.CreateJsonWebToken(idToken, request.Client, _configuration);

            var accessTokenReference = _tokenHandles.Store(accessToken);

            return new TokenResponse
            {
                Jwt = jwt,
                AccessTokenReference = accessTokenReference,
                AccessTokenLifetime = accessToken.Lifetime
            };
        }
    }
}
