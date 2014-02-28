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
            if (request.GrantType == Constants.GrantTypes.AuthorizationCode)
            {
                return ProcessAuthorizationCodeRequest(request);
            }
            else if (request.GrantType == Constants.GrantTypes.ClientCredentials)
            {
                return ProcessClientCredentialsRequest(request, client);
            }

            throw new NotImplementedException();
        }

        private TokenResponse ProcessAuthorizationCodeRequest(ValidatedTokenRequest request)
        {
            var response = new TokenResponse
            {
                AccessToken = CreateAccessToken(request),
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            if (request.AuthorizationCode.IsOpenId)
            {
                var idToken = _tokenService.CreateIdentityToken(request, null);

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

                response.IdentityToken = jwt;
            }

            return response;
        }

        private TokenResponse ProcessClientCredentialsRequest(ValidatedTokenRequest request, ClaimsPrincipal clientPrincipal)
        {
            var response = new TokenResponse
            {
                AccessToken = CreateAccessToken(request),
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            return response;
        }

        private string CreateAccessToken(ValidatedTokenRequest request)
        {
            var accessToken = _tokenService.CreateAccessToken(request, null);

            if (request.Client.AccessTokenType == AccessTokenType.JWT)
            {
                return _tokenService.CreateJsonWebToken(
                    accessToken,
                    new X509SigningCredentials(_settings.GetSigningCertificate()));
            }
            else
            {
                var reference = Guid.NewGuid().ToString("N");
                _tokenHandles.Store(reference, accessToken);

                return reference;
            }
        }
    }
}
