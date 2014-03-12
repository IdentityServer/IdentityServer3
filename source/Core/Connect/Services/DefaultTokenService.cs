using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private IClaimsProvider _claimsProvider;

        public DefaultTokenService(IUserService profile, ICoreSettings settings, IClaimsProvider claimsProvider)
        {
            _profile = profile;
            _settings = settings;
            _claimsProvider = claimsProvider;
        }

        public Token CreateIdentityToken(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, bool includeAllIdentityClaims, NameValueCollection request)
        {
            // host provided claims
            var claims = new List<Claim>(subject.Claims);
            
            // if nonce was sent, must be mirrored in id token
            var nonce = request.Get(Constants.AuthorizeRequest.Nonce);
            if (nonce.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.Nonce, nonce));
            }

            claims.AddRange(_claimsProvider.GetIdentityTokenClaims(
                subject,
                client,
                scopes,
                _settings,
                includeAllIdentityClaims,
                _profile,
                request));

            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = client.ClientId,
                Issuer = _settings.GetIssuerUri(),
                Lifetime = client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList()
            };

            return token;
        }

        public virtual Token CreateAccessToken(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, NameValueCollection request)
        {
            var claims = _claimsProvider.GetAccessTokenClaims(
                subject,
                client,
                scopes,
                _settings,
                _profile,
                request);

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