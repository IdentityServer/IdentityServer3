using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
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
            var token = new Token
            {
                Audience = request.ClientId,
                Issuer = _settings.GetIssuerUri(),
                Lifetime = request.Client.IdentityTokenLifetime,
                Type = Constants.TokenTypes.IdentityToken,
                Claims = user.Claims.ToList()
            };

            if (request.Nonce.IsPresent())
            {
                token.Claims.Add(new Claim(Constants.ClaimTypes.Nonce, request.Nonce));
            }

            if (!request.AccessTokenRequested)
            {
                var requestedClaimTypes = new List<string>();
                foreach (var scope in request.Scopes)
                {
                    requestedClaimTypes.AddRange(Constants.ScopeToClaimsMapping[scope]);
                }

                token.Claims.AddRange(_profile.GetProfileData(user.GetSubject(), requestedClaimTypes));
            }

            return token;
        }

        public virtual Token CreateAccessToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            var token = new Token
            {
                Audience = _settings.GetIssuerUri() + "/resources",
                Issuer = _settings.GetIssuerUri(),
                Lifetime = request.Client.AccessTokenLifetime,
                Type = Constants.TokenTypes.AccessToken,

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