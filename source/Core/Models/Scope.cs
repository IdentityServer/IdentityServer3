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

using System.Collections.Generic;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Models are resource (either identity resource or web api resource)
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// Indicates if scope is enabled and can be requested. Defaults to true.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Name of the scope. This is the value a client will use to request the scope.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Display name. This value will be used e.g. on the consent screen.
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Description. This value will be used e.g. on the consent screen.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
        /// </summary>
        public bool Emphasize { get; set; }

        /// <summary>
        /// Specifies whether this scope is about identity information from the userinfo endpoint, or a resource (e.g. a Web API). Defaults to Resource.
        /// </summary>
        public ScopeType Type { get; set; }
        
        /// <summary>
        /// List of user claims that should be included in the identity (identity scope) or access token (resource scope). 
        /// </summary>
        public List<ScopeClaim> Claims { get; set; }
        
        /// <summary>
        /// If enabled, all claims for the user will be included in the token. Defaults to false.
        /// </summary>
        public bool IncludeAllClaimsForUser { get; set; }
        
        /// <summary>
        /// Rule for determining which claims should be included in the token (this is implementation specific)
        /// </summary>
        public string ClaimsRule { get; set; }

        /// <summary>
        /// Specifies whether this scope is shown in the discovery document. Defaults to true.
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; }

        /// <summary>
        /// Creates a Scope with default values
        /// </summary>
        public Scope()
        {
            Type = ScopeType.Resource;
            Claims = new List<ScopeClaim>();
            IncludeAllClaimsForUser = false;
            Enabled = true;
            ShowInDiscoveryDocument = true;
        }
    }
}