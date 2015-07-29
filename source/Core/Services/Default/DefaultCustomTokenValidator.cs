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

using IdentityModel;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default custom token validator
    /// </summary>
    public class DefaultCustomTokenValidator : ICustomTokenValidator
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// The user service
        /// </summary>
        protected readonly IUserService _users;

        /// <summary>
        /// The client store
        /// </summary>
        protected readonly IClientStore _clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCustomTokenValidator"/> class.
        /// </summary>
        /// <param name="users">The users store.</param>
        /// <param name="clients">The client store.</param>
        public DefaultCustomTokenValidator(IUserService users, IClientStore clients)
        {
            _users = users;
            _clients = clients;
        }

        /// <summary>
        /// Custom validation logic for access tokens.
        /// </summary>
        /// <param name="result">The validation result so far.</param>
        /// <returns>
        /// The validation result
        /// </returns>
        public virtual async Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result)
        {
            if (result.IsError)
            {
                return result;
            }

            // make sure user is still active (if sub claim is present)
            var subClaim = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
            if (subClaim != null)
            {
                var principal = Principal.Create("tokenvalidator", result.Claims.ToArray());

                if (result.ReferenceTokenId.IsPresent())
                {
                    principal.Identities.First().AddClaim(new Claim(Constants.ClaimTypes.ReferenceTokenId, result.ReferenceTokenId));
                }

                var isActiveCtx = new IsActiveContext(principal, result.Client);
                await _users.IsActiveAsync(isActiveCtx);
                
                if (isActiveCtx.IsActive == false)
                {
                    Logger.Warn("User marked as not active: " + subClaim.Value);

                    result.IsError = true;
                    result.Error = Constants.ProtectedResourceErrors.InvalidToken;
                    result.Claims = null;

                    return result;
                }
            }

            // make sure client is still active (if client_id claim is present)
            var clientClaim = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.ClientId);
            if (clientClaim != null)
            {
                var client = await _clients.FindClientByIdAsync(clientClaim.Value);
                if (client == null || client.Enabled == false)
                {
                    Logger.Warn("Client deleted or disabled: " + clientClaim.Value);

                    result.IsError = true;
                    result.Error = Constants.ProtectedResourceErrors.InvalidToken;
                    result.Claims = null;

                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Custom validation logic for identity tokens.
        /// </summary>
        /// <param name="result">The validation result so far.</param>
        /// <returns>
        /// The validation result
        /// </returns>
        public virtual async Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result)
        {
            // make sure user is still active (if sub claim is present)
            var subClaim = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
            if (subClaim != null)
            {
                var principal = Principal.Create("tokenvalidator", result.Claims.ToArray());

                var isActiveCtx = new IsActiveContext(principal, result.Client);
                await _users.IsActiveAsync(isActiveCtx);
                
                if (isActiveCtx.IsActive == false)
                {
                    Logger.Warn("User marked as not active: " + subClaim.Value);

                    result.IsError = true;
                    result.Error = Constants.ProtectedResourceErrors.InvalidToken;
                    result.Claims = null;

                    return result;
                }
            }

            return result;
        }
    }
}