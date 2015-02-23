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

using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    /// <summary>
    /// Abstraction for client secret validation
    /// </summary>
    public interface IClientSecretValidator
    {
        /// <summary>
        /// Validates the client secret.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="secret">The secret.</param>
        /// <returns><c>true</c> if the secret is valid; <c>false</c> otherwise.</returns>
        Task<bool> ValidateClientSecretAsync(Client client, string secret);
    }
}