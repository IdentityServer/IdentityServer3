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
using System.Linq;
using Thinktecture.IdentityServer.Core.Resources;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class Scope
    {
        public bool Enabled { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public bool Emphasize { get; set; }

        public ScopeType Type { get; set; }
        public IEnumerable<ScopeClaim> Claims { get; set; }

        public Scope()
        {
            Type = ScopeType.Resource;
            Claims = new ScopeClaim[] { };
            Enabled = true;
        }

        public static IEnumerable<Scope> StandardScopes
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

        public static Scope OpenId
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.OpenId,
                    DisplayName = Scopes.OpenIdDisplayName,
                    Required = true,
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Subject, alwaysInclude: true)
                    }
                };
            }
        }

        public static Scope Profile
        {
            get
            {
                return new Scope
                 {
                     Name = Constants.StandardScopes.Profile,
                     DisplayName = Scopes.ProfileDisplayName,
                     Description = Scopes.ProfileDescription,
                     Type = ScopeType.Identity,
                     Emphasize = true,
                     Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(claim => new ScopeClaim(claim)))
                 };
            }
        }

        public static Scope Email
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Email,
                    DisplayName = Scopes.EmailDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Email].Select(claim => new ScopeClaim(claim)))
                };
            }
        }

        public static Scope Phone
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Phone,
                    DisplayName = Scopes.ProfileDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Phone].Select(claim => new ScopeClaim(claim)))
                };
            }
        }

        public static Scope Address
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Address,
                    DisplayName = Scopes.AddressDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Address].Select(claim => new ScopeClaim(claim)))
                };
            }
        }

        public static Scope OfflineAccess
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.OfflineAccess,
                    DisplayName = Scopes.OfflineAccessDisplayName,
                    Type = ScopeType.Resource,
                    Emphasize = true
                };
            }
        }
    }
}