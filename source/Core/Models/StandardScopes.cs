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
using System.Linq;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// Convenience class that defines standard identity scopes.
    /// </summary>
    public static class StandardScopes
    {
        /// <summary>
        /// All identity scopes.
        /// </summary>
        /// <value>
        /// All.
        /// </value>
        public static IEnumerable<Scope> All
        {
            get
            {
                return new[]
                {
                    OpenId,
                    Profile,
                    Email,
                    Phone,
                    Address
                };
            }
        }

        /// <summary>
        /// All identity scopes (always include claims in token).
        /// </summary>
        /// <value>
        /// All always include.
        /// </value>
        public static IEnumerable<Scope> AllAlwaysInclude
        {
            get
            {
                return new[]
                {
                    OpenId,
                    ProfileAlwaysInclude,
                    EmailAlwaysInclude,
                    PhoneAlwaysInclude,
                    AddressAlwaysInclude
                };
            }
        }


        /// <summary>
        /// Gets the "openid" scope.
        /// </summary>
        /// <value>
        /// The open identifier.
        /// </value>
        public static Scope OpenId
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.OpenId,
                    Required = true,
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Subject, alwaysInclude: true)
                    }
                };
            }
        }

        /// <summary>
        /// Gets the "profile" scope.
        /// </summary>
        /// <value>
        /// The profile.
        /// </value>
        public static Scope Profile
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Profile,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(claim => new ScopeClaim(claim)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "profile" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The profile always include.
        /// </value>
        public static Scope ProfileAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Profile,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(claim => new ScopeClaim(claim, alwaysInclude: true)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "email" scope.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public static Scope Email
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Email,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Email].Select(claim => new ScopeClaim(claim)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "email" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The email always include.
        /// </value>
        public static Scope EmailAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Email,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Email].Select(claim => new ScopeClaim(claim, alwaysInclude: true)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "phone" scope.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        public static Scope Phone
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Phone,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Phone].Select(claim => new ScopeClaim(claim)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "phone" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The phone always include.
        /// </value>
        public static Scope PhoneAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Phone,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Phone].Select(claim => new ScopeClaim(claim, alwaysInclude: true)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "address" scope.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public static Scope Address
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Address,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Address].Select(claim => new ScopeClaim(claim)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "address" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The address always include.
        /// </value>
        public static Scope AddressAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Address,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Address].Select(claim => new ScopeClaim(claim, alwaysInclude: true)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "all_claims" scope.
        /// </summary>
        /// <value>
        /// All claims.
        /// </value>
        public static Scope AllClaims
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.AllClaims,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    IncludeAllClaimsForUser = true
                };
            }
        }

        /// <summary>
        /// Gets the "roles" scope.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public static Scope Roles
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Roles,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = new List<ScopeClaim> 
                    {
                        new ScopeClaim(Constants.ClaimTypes.Role)
                    }
                };
            }
        }

        /// <summary>
        /// Gets the "roles" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The roles always include.
        /// </value>
        public static Scope RolesAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Roles,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Role, alwaysInclude: true)
                    }
                };
            }
        }

        /// <summary>
        /// Gets the "offline_access" scope.
        /// </summary>
        /// <value>
        /// The offline access.
        /// </value>
        public static Scope OfflineAccess
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.OfflineAccess,
                    Type = ScopeType.Resource,
                    Emphasize = true
                };
            }
        }
    }
}