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

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Models a claim that should be emitted in a token
    /// </summary>
    public class ScopeClaim
    {
        /// <summary>
        /// Name of the claim
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the claim
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only. Defaults to false.
        /// </summary>
        public bool AlwaysIncludeInIdToken { get; set; }

        /// <summary>
        /// Creates an empty ScopeClaim
        /// </summary>
        public ScopeClaim()
        { }

        /// <summary>
        /// Creates a ScopeClaim with parameters
        /// </summary>
        /// <param name="name">Name of the claim</param>
        /// <param name="alwaysInclude">Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only.</param>
        public ScopeClaim(string name, bool alwaysInclude = false)
        {
            Name = name;
            Description = string.Empty;
            AlwaysIncludeInIdToken = alwaysInclude;
        }
    }
}