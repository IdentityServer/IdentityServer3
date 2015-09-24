using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.InMemory
{
    /// <summary>
    /// CORS policy service that configures the allowed origins from a list of clients' redirect URLs.
    /// </summary>
    public class InMemoryCorsPolicyService : ICorsPolicyService
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        readonly IEnumerable<Client> clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCorsPolicyService"/> class.
        /// </summary>
        /// <param name="clients">The clients.</param>
        public InMemoryCorsPolicyService(IEnumerable<Client> clients)
        {
            this.clients = clients ?? Enumerable.Empty<Client>();
        }

        /// <summary>
        /// Determines whether origin is allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            var query =
                from client in clients
                from url in client.AllowedCorsOrigins
                select url.GetOrigin();

            var result = query.Contains(origin, StringComparer.OrdinalIgnoreCase);

            if (result)
            {
                Logger.InfoFormat("Client list checked and origin: {0} is allowed", origin);
            }
            else
            {
                Logger.InfoFormat("Client list checked and origin: {0} is not allowed", origin);
            }
            
            return Task.FromResult(result);
        }
    }
}
