/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultTokenService : ITokenService
    {
        private IUserService _users;
        private CoreSettings _settings;
        private IClaimsProvider _claimsProvider;
        private ITokenHandleStore _tokenHandles;

        public DefaultTokenService(IUserService users, CoreSettings settings, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles)
        {
            _users = users;
            _settings = settings;
            _claimsProvider = claimsProvider;
            _tokenHandles = tokenHandles;
        }

        public virtual async Task<Token> CreateIdentityTokenAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, bool includeAllIdentityClaims, NameValueCollection request, string accessTokenToHash = null)
        {
            // host provided claims
            var claims = new List<Claim>();
            
            // if nonce was sent, must be mirrored in id token
            var nonce = request.Get(Constants.AuthorizeRequest.Nonce);
            if (nonce.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.Nonce, nonce));
            }

            // add iat claim
            claims.Add(new Claim(Constants.ClaimTypes.IssuedAt, DateTime.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer));

            // add at_hash claim
            if (accessTokenToHash.IsPresent())
            {
                claims.Add(new Claim(Constants.ClaimTypes.AccessTokenHash, HashAccessToken(accessTokenToHash)));
            }

            claims.AddRange(await _claimsProvider.GetIdentityTokenClaimsAsync(
                subject,
                client,
                scopes,
                _settings,
                includeAllIdentityClaims,
                _users,
                request));

            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = client.ClientId,
                Issuer = _settings.GetIssuerUri(),
                Lifetime = client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                Client = client
            };

            return token;
        }

        public virtual async Task<Token> CreateAccessTokenAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, NameValueCollection request)
        {
            var claims = await _claimsProvider.GetAccessTokenClaimsAsync(
                subject,
                client,
                scopes,
                _settings,
                _users,
                request);

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = string.Format(Constants.AccessTokenAudience, _settings.GetIssuerUri()),
                Issuer = _settings.GetIssuerUri(),
                Lifetime = client.AccessTokenLifetime,
                Claims = claims.ToList(),
                Client = client
            };

            return token;
        }

        public virtual async Task<string> CreateSecurityTokenAsync(Token token)
        {
            if (token.Type == Constants.TokenTypes.AccessToken)
            {
                if (token.Client.AccessTokenType == AccessTokenType.JWT)
                {
                    return CreateJsonWebToken(
                        token,
                        new X509SigningCredentials(_settings.GetSigningCertificate()));
                }
                else
                {
                    var handle = Guid.NewGuid().ToString("N");
                    await _tokenHandles.StoreAsync(handle, token);

                    return handle;
                }
            }

            if (token.Type == Constants.TokenTypes.IdentityToken)
            {
                SigningCredentials credentials;
                if (token.Client.IdentityTokenSigningKeyType == SigningKeyTypes.ClientSecret)
                {
                    credentials = new HmacSigningCredentials(token.Client.ClientSecret);
                }
                else
                {
                    credentials = new X509SigningCredentials(_settings.GetSigningCertificate());
                }

                return CreateJsonWebToken(token, credentials);
            }

            throw new InvalidOperationException("Invalid token type.");
        }

        protected virtual string CreateJsonWebToken(Token token, SigningCredentials credentials)
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

        protected virtual string HashAccessToken(string accessTokenToHash)
        {
            var algorithm = SHA256.Create();
            var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(accessTokenToHash));

            var leftPart = new byte[16];
            Array.Copy(hash, leftPart, 16);

            return Base64Url.Encode(leftPart);
        }
    }
}