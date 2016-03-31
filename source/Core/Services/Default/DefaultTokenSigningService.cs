/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default token signing service
    /// </summary>
    public class DefaultTokenSigningService : ITokenSigningService
    {
        /// <summary>
        /// The identity server options
        /// </summary>
        protected readonly IdentityServerOptions _options;

        /// <summary>
        /// The signing key service
        /// </summary>
        private readonly ISigningKeyService _keyService;

        // todo: remove in next major version
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenSigningService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DefaultTokenSigningService(IdentityServerOptions options)
        {
            _options = options;
            _keyService = new DefaultSigningKeyService(options);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenSigningService"/> class.
        /// </summary>
        /// <param name="keyService">The signing key service.</param>
        public DefaultTokenSigningService(ISigningKeyService keyService)
        {
            _keyService = keyService;
        }

        /// <summary>
        /// Signs the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// A protected and serialized security token
        /// </returns>
        public virtual async Task<string> SignTokenAsync(Token token)
        {
            var credentials = await GetSigningCredentialsAsync();
            return await CreateJsonWebToken(token, credentials);
        }

        /// <summary>
        /// Retrieves the signing credential (override to load key from alternative locations)
        /// </summary>
        /// <returns>The signing credential</returns>
        protected virtual async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            return new X509SigningCredentials(await _keyService.GetSigningKeyAsync());
        }

        /// <summary>
        /// Creates the json web token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>The signed JWT</returns>
        protected virtual async Task<string> CreateJsonWebToken(Token token, SigningCredentials credentials)
        {
            var payload = CreatePayload(token);
            return await SignAsync(payload, credentials);
        }

        /// <summary>
        /// Creates the JWT payload
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The JWT payload</returns>
        protected virtual string CreatePayload(Token token)
        {
            return token.CreateJwtPayload();   
        }

        /// <summary>
        /// Creates the JWT header
        /// </summary>
        /// <param name="credential">The credentials.</param>
        /// <returns>The JWT header</returns>
        private async Task<JwtHeader> CreateHeaderAsync(SigningCredentials credential)
        {
            var header = new JwtHeader(credential);

            var x509credential = credential as X509SigningCredentials;
            if (x509credential != null)
            {
                header.Add("kid", await _keyService.GetKidAsync(x509credential.Certificate));
            }

            return header;
        }

        private async Task<string> SignAsync(string payload, SigningCredentials credentials)
        {
            var header = await CreateHeaderAsync(credentials);
            var jwtPayload = JwtPayload.Deserialize(payload);

            var token = new JwtSecurityToken(header, jwtPayload);

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }
    }
}