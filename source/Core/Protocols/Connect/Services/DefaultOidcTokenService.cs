using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect.Services
{
    public class DefaultOidcTokenService : IOidcTokenService
    {
        private IProfileService _profile;

        public DefaultOidcTokenService(IProfileService profile)
        {
            _profile = profile;
        }

        public virtual Token CreateIdentityToken(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            var token = new Token
            {
                Audience = request.ClientId,
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
                Audience = "userinfo",
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

        public virtual string CreateJsonWebToken(Token token, OidcClient client, OidcConfiguration configuration)
        {
            // todo: sig key strategy??
            var signingCredentials = new X509SigningCredentials(configuration.SigningKey);

            var jwt = new JwtSecurityToken(
                configuration.IssuerName,
                token.Audience,
                token.Claims,
                new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddSeconds(token.Lifetime)),
                signingCredentials);

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(jwt);
        }


        public virtual Token CreateIdentityToken(ValidatedTokenRequest request, ClaimsPrincipal user)
        {
            var token = new Token
            {
                Audience = request.Client.ClientId,
                Lifetime = request.Client.IdentityTokenLifetime,
                Type = Constants.TokenTypes.IdentityToken,
                Claims = user.Claims.ToList()
            };

            return token;
        }

        public virtual Token CreateAccessToken(ValidatedTokenRequest request, ClaimsPrincipal user)
        {
            var token = new Token
            {
                Audience = "userinfo",
                Lifetime = request.Client.AccessTokenLifetime,
                Type = Constants.TokenTypes.AccessToken,

                Claims = new List<Claim>
                {
                    new Claim(Constants.ClaimTypes.Subject, user.GetSubject()),
                    new Claim(Constants.ClaimTypes.ClientId, request.Client.ClientId),
                    new Claim(Constants.ClaimTypes.Scope, request.AuthorizationCode.RequestedScopes)
                }
            };

            return token;
        }
    }
}