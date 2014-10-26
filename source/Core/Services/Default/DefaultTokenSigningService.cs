/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
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

            if (token.Type == Constants.TokenTypes.IdentityToken &&
                token.Client.IdentityTokenSigningKeyType == SigningKeyTypes.ClientSecret)
            {
                return Task.FromResult(CreateJsonWebToken(token, new HmacSigningCredentials(token.Client.ClientSecret)));
            }

            throw new InvalidOperationException("Invalid token type");
        }

        protected virtual string CreateJsonWebToken(Token token, SigningCredentials credentials)
        {
            var jwt = new JwtSecurityToken(
                token.Issuer,
                token.Audience,
                token.Claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddSeconds(token.Lifetime),
                credentials);

            var x509credential = credentials as X509SigningCredentials;
            if (x509credential != null)
            {
                jwt.Header.Add("kid", Base64Url.Encode(x509credential.Certificate.GetCertHash()));
            }

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(jwt);
        }
    }
}