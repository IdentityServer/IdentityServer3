using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Models
{
    public static class StandardScopes
    {
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

        public static Scope OpenId
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.OpenId,
                    DisplayName = Resources.Scopes.OpenIdDisplayName,
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
                    DisplayName = Resources.Scopes.ProfileDisplayName,
                    Description = Resources.Scopes.ProfileDescription,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(claim => new ScopeClaim(claim)))
                };
            }
        }

        public static Scope ProfileAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Profile,
                    DisplayName = Resources.Scopes.ProfileDisplayName,
                    Description = Resources.Scopes.ProfileDescription,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(claim => new ScopeClaim(claim, alwaysInclude: true)))
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
                    DisplayName = Resources.Scopes.EmailDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Email].Select(claim => new ScopeClaim(claim)))
                };
            }
        }

        public static Scope EmailAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Email,
                    DisplayName = Resources.Scopes.EmailDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Email].Select(claim => new ScopeClaim(claim, alwaysInclude: true)))
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
                    DisplayName = Resources.Scopes.ProfileDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Phone].Select(claim => new ScopeClaim(claim)))
                };
            }
        }

        public static Scope PhoneAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Phone,
                    DisplayName = Resources.Scopes.ProfileDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Phone].Select(claim => new ScopeClaim(claim, alwaysInclude: true)))
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
                    DisplayName = Resources.Scopes.AddressDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Address].Select(claim => new ScopeClaim(claim)))
                };
            }
        }


        public static Scope AddressAlwaysInclude
        {
            get
            {
                return new Scope
                {
                    Name = Constants.StandardScopes.Address,
                    DisplayName = Resources.Scopes.AddressDisplayName,
                    Type = ScopeType.Identity,
                    Emphasize = true,
                    Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Address].Select(claim => new ScopeClaim(claim, alwaysInclude: true)))
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
                    DisplayName = Resources.Scopes.OfflineAccessDisplayName,
                    Type = ScopeType.Resource,
                    Emphasize = true
                };
            }
        }
    }
}