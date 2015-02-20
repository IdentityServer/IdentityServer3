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
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default CORS policy service.
    /// </summary>
    public class DefaultCorsPolicyService : ICorsPolicyService
    {
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
        
        Func<string, Task<bool>> policyCallback;
        
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
                return true;
            }

            if (AllowedOrigins != null)
            {
                if (AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (policyCallback != null)
            {
                if (await policyCallback(origin))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
