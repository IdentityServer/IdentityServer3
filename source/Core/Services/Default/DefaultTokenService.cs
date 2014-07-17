/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Plumbing;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class DefaultTokenService : ITokenService
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions _options;
        private readonly IClaimsProvider _claimsProvider;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly ITokenSigningService _signingService;

        public DefaultTokenService(IdentityServerOptions options, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles, ITokenSigningService signingService)
        {
            _options = options;
            _claimsProvider = claimsProvider;
            _tokenHandles = tokenHandles;
            _signingService = signingService;
        }

        public virtual async Task<Token> CreateIdentityTokenAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, bool includeAllIdentityClaims, NameValueCollection request, string accessTokenToHash = null)
        {
            Logger.Debug("Creating identity token");

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
                includeAllIdentityClaims,
                request));

            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = client.ClientId,
                Issuer = _options.IssuerUri,
                Lifetime = client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                Client = client
            };

            return token;
        }

        public virtual async Task<Token> CreateAccessTokenAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, NameValueCollection request)
        {
            Logger.Debug("Creating access token");

            var claims = await _claimsProvider.GetAccessTokenClaimsAsync(
                subject,
                client,
                scopes,
                request);

            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = string.Format(Constants.AccessTokenAudience, _options.IssuerUri),
                Issuer = _options.IssuerUri,
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
                if (token.Client.AccessTokenType == AccessTokenType.Jwt)
                {
                    Logger.Debug("Creating JWT access token");

                    return await _signingService.SignTokenAsync(token);
                }
                else
                {
                    Logger.Debug("Creating reference access token");

                    var handle = Guid.NewGuid().ToString("N");
                    await _tokenHandles.StoreAsync(handle, token);

                    return handle;
                }
            }

            if (token.Type == Constants.TokenTypes.IdentityToken)
            {
                Logger.Debug("Creating JWT identity token");

                return await _signingService.SignTokenAsync(token);
            }

            throw new InvalidOperationException("Invalid token type.");
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