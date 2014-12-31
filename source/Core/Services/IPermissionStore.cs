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
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    /// <summary>
    /// Models an abstraction for querying and revoking a subject's permissions for clients. This abstraction
    /// aggregates permissions for access tokens, refresh tokens, codes, and consent.
    /// </summary>
    public interface IPermissionsStore
    {
        /// <summary>
        /// Loads all permissions the subject has granted to all clients.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>The permissions.</returns>
        Task<IEnumerable<Consent>> LoadAllAsync(string subject);
        
        /// <summary>
        /// Revokes all permissions the subject has given to a client.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        Task RevokeAsync(string subject, string client);
    }
}