using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Services;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect
{
    public class OidcTokenResponseGenerator
    {
        private ITokenService _tokenService;
        private ITokenHandleService _tokenHandles;
        private Configuration _configuration;
        
        public OidcTokenResponseGenerator(ITokenService tokenService, ITokenHandleService tokenHandles, Configuration configuration)
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
