using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultTokenService : ITokenService
    {
        private IUserService _profile;
        private ICoreSettings _settings;

        public DefaultTokenService(IUserService profile, ICoreSettings settings)
        {
            _profile = profile;
            _settings = settings;
        }

        public virtual Token CreateIdentityToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, user.GetSubject()),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, user.GetAuthenticationMethod()),
                new Claim(Constants.ClaimTypes.AuthenticationTime, user.GetAuthenticationTimeEpoch().ToString())
            };

            if (request.Nonce.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.Nonce, request.Nonce));
            }

            // todo: must share logic with userinfo endpoint
            if (!request.AccessTokenRequested)
            {
                var requestedClaimTypes = new List<string>();
                var scopeDetails = _settings.GetScopes();

                foreach (var scope in request.Scopes)
                {
                    var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);

                    if (scopeDetail != null)
                    {
                        if (scopeDetail.IsOpenIdScope)
                        {
                            requestedClaimTypes.AddRange(scopeDetail.Claims.Select(c => c.Name));
                        }
                    }
                }

                claims.AddRange(_profile.GetProfileData(user.GetSubject(), requestedClaimTypes));
            }

            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = request.ClientId,
                Issuer = _settings.GetIssuerUri(),
                Lifetime = request.Client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList()
            };

            return token;
        }

        public virtual Token CreateAccessToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = _settings.GetIssuerUri() + "/resources",
                Issuer = _settings.GetIssuerUri(),
                Lifetime = request.Client.AccessTokenLifetime,

                Claims = new List<Claim>
                {
                    new Claim(Constants.ClaimTypes.Subject, user.GetSubject()),
                    new Claim(Constants.ClaimTypes.ClientId, request.ClientId),
                    new Claim(Constants.ClaimTypes.Scope, request.Scopes.ToSpaceSeparatedString())
                }
            };

            return token;
        }

        public virtual string CreateJsonWebToken(Token token, SigningCredentials credentials)
        {
            var jwt = new JwtSecurityToken(
                token.Issuer,
                token.Audience,
                token.Claims,
                new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddSeconds(token.Lifetime)),
                credentials);

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(jwt);
        }

        public virtual Token CreateIdentityToken(ValidatedTokenRequest request, ClaimsPrincipal user)
        {
            return request.AuthorizationCode.IdentityToken;
        }

        public virtual Token CreateAccessToken(ValidatedTokenRequest request, ClaimsPrincipal user)
        {
            return request.AuthorizationCode.AccessToken;
        }
    }
}
