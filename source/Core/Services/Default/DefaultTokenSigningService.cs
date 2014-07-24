/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    // todo: logging
    public class DefaultTokenSigningService : ITokenSigningService
    {
        private readonly IdentityServerOptions _options;

        public DefaultTokenSigningService(IdentityServerOptions options)
        {
            _options = options;
        }

        public Task<string> SignTokenAsync(Token token)
        {
            if (token.Type == Constants.TokenTypes.AccessToken ||
               (token.Type == Constants.TokenTypes.IdentityToken && 
                token.Client.IdentityTokenSigningKeyType == SigningKeyTypes.Default))
            {
                return Task.FromResult(CreateJsonWebToken(token, new X509SigningCredentials(_options.SigningCertificate)));
            }
            else
            {
                if (token.Type == Constants.TokenTypes.IdentityToken &&
                    token.Client.IdentityTokenSigningKeyType == SigningKeyTypes.ClientSecret)
                {
                    return Task.FromResult(CreateJsonWebToken(token, new HmacSigningCredentials(token.Client.ClientSecret)));
                }
            }

            throw new InvalidOperationException("Invalid token type");
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
    }
}