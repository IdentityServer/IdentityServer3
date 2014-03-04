using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultTokenService : ITokenService
    {
        private IUserService _profile;
        private ICoreSettings _settings;
        private IClaimsProvider _claimsProvider;

        public DefaultTokenService(IUserService profile, ICoreSettings settings, IClaimsProvider claimsProvider)
        {
            _profile = profile;
            _settings = settings;
            _claimsProvider = claimsProvider;
        }

        public virtual Token CreateIdentityToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            // minimal, mandatory claims
            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, user.GetSubject()),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, user.GetAuthenticationMethod()),
                new Claim(Constants.ClaimTypes.AuthenticationTime, user.GetAuthenticationTimeEpoch().ToString(), ClaimValueTypes.Integer),
                new Claim(Constants.ClaimTypes.IssuedAt, DateTime.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer)
            };

            // if nonce was sent, must be mirrored in id token
            if (request.Nonce.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.Nonce, request.Nonce));
            }

            claims.AddRange(_claimsProvider.GetIdentityTokenClaims(
                user,
                request.Client,
                request.ValidatedScopes.GrantedScopes,
                _settings,
                !request.AccessTokenRequested,
                _profile));

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
            var claims = _claimsProvider.GetAccessTokenClaims(
                user,
                request.Client,
                request.ValidatedScopes.GrantedScopes,
                _settings,
                _profile);

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = _settings.GetIssuerUri() + "/resources",
                Issuer = _settings.GetIssuerUri(),
                Lifetime = request.Client.AccessTokenLifetime,
                Claims = claims.ToList()
            };

            return token;
        }

        public virtual Token CreateAccessToken(ValidatedTokenRequest request)
        {
            return CreateAccessToken(request.Subject, request.Client, request.ValidatedScopes);
        }

        public virtual Token CreateAccessToken(ClaimsPrincipal user, Client client, ScopeValidator scopes)
        {
            var claims = _claimsProvider.GetAccessTokenClaims(
                user,
                client,
                scopes.GrantedScopes,
                _settings,
                _profile);

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = _settings.GetIssuerUri() + "/resources",
                Issuer = _settings.GetIssuerUri(),
                Lifetime = client.AccessTokenLifetime,
                Claims = claims.ToList()
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
    }
}