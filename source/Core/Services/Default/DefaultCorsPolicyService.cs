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
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default CORS policy service.
    /// </summary>
    public class DefaultCorsPolicyService : ICorsPolicyService
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCorsPolicyService"/> class.
        /// </summary>
        public DefaultCorsPolicyService()
        {
            AllowedOrigins = new HashSet<string>();
        }

        /// <summary>
        /// The list allowed origins that are allowed.
        /// </summary>
        /// <value>
        /// The allowed origins.
        /// </value>
        public ICollection<string> AllowedOrigins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all origins are allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allow all; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAll { get; set; }

        readonly Func<string, Task<bool>> policyCallback;
        
        internal DefaultCorsPolicyService(CorsPolicy policy)
        {
            if (policy == null) throw new ArgumentNullException("policy");
            
            AllowedOrigins = policy.AllowedOrigins;
            policyCallback = policy.PolicyCallback;
        }

        /// <summary>
        /// Determines whether the origin allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public async Task<bool> IsOriginAllowedAsync(string origin)
        {
            if (AllowAll)
            {
                Logger.InfoFormat("AllowAll true, so origin: {0} is allowed", origin);
                return true;
            }

            if (AllowedOrigins != null)
            {
                if (AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    Logger.InfoFormat("AllowedOrigins configured and origin {0} is allowed", origin);
                    return true;
                }
                else
                {
                    Logger.InfoFormat("AllowedOrigins configured and origin {0} is not allowed", origin);
                }
            }

            if (policyCallback != null)
            {
                if (await policyCallback(origin))
                {
                    Logger.InfoFormat("policyCallback callback invoked and origin {0} is allowed", origin);
                    return true;
                }
                else
                {
                    Logger.InfoFormat("policyCallback callback invoked and origin {0} is not allowed", origin);
                }
            }

            Logger.InfoFormat("Exiting; origin {0} is not allowed", origin);

            return false;
        }
    }
}
