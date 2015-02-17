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
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services.Default;

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    /// <summary>
    /// CORS policy service that configures the allowed origins from a list of clients' redirect URLs.
    /// </summary>
    public class InMemoryClientCorsPolicyService : DefaultCorsPolicyService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryClientCorsPolicyService"/> class.
        /// </summary>
        /// <param name="clients">The clients.</param>
        public InMemoryClientCorsPolicyService(IEnumerable<Client> clients)
        {
            clients = clients ?? Enumerable.Empty<Client>();

            var origins = GetOriginsToAllow(clients);
            
            if (origins != null)
            {
                foreach(var origin in origins.Distinct())
                {
                    if (origin != null)
                    {
                        AllowedOrigins.Add(origin);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the origins to allow from the clients.
        /// </summary>
        /// <param name="clients">The clients.</param>
        /// <returns></returns>
        protected virtual IEnumerable<string> GetOriginsToAllow(IEnumerable<Client> clients)
        {
            var query =
                from client in clients
                where client.Flow == Flows.Hybrid || client.Flow == Flows.Implicit
                from url in client.RedirectUris
                select GetOrigin(url);
            return query;
        }

        /// <summary>
        /// Gets the origin from the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        protected string GetOrigin(string url)
        {
            if (url != null && (url.StartsWith("http://") || url.StartsWith("https://")))
            {
                var idx = url.IndexOf("//");
                if (idx > 0)
                {
                    idx = url.IndexOf("/", idx + 2);
                    if (idx >= 0)
                    {
                        url = url.Substring(0, idx);
                    }
                    return url;
                }
            }
            
            return null;
        }
    }
}
