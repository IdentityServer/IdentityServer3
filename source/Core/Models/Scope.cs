/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class Scope
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public bool Emphasize { get; set; }
        public bool IsOpenIdScope { get; set; }
        public IEnumerable<ScopeClaim> Claims { get; set; }

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
                    DisplayName = "Your user identifier",
                    Required = true,
                    IsOpenIdScope = true,
                    Claims = new List<ScopeClaim>
                        {
                            new ScopeClaim
                            {
                                AlwaysIncludeInIdToken = true,
                                Name = Constants.ClaimTypes.Subject,
                                Description = "subject identifier"
                            }
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
                     DisplayName = "User profile",
                     Description = "Your user profile information (first name, last name, etc.).",
                     IsOpenIdScope = true,
                     Emphasize = true,
                     Claims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(x => new ScopeClaim { Name = x, Description = x }))
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
                    DisplayName = "Your email address",
                    IsOpenIdScope = true,
                    Emphasize = true,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim
                        {
                            Name = Constants.ClaimTypes.Email,
                            Description = "email address",
                        },
                        new ScopeClaim
                        {
                            Name = Constants.ClaimTypes.EmailVerified,
                            Description = "email is verified",
                        }
                    }
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
                    DisplayName = "Your phone number",
                    IsOpenIdScope = true,
                    Emphasize = true,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim
                        {
                            Name = Constants.ClaimTypes.PhoneNumber,
                            Description = "phone number",
                        },
                        new ScopeClaim
                        {
                            Name = Constants.ClaimTypes.PhoneNumberVerified,
                            Description = "phone number is verified",
                        }
                    }
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
                    DisplayName = "Your postal address",
                    IsOpenIdScope = true,
                    Emphasize = true,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim
                        {
                            Name = Constants.ClaimTypes.Address,
                            Description = "Your postal address",
                        }
                    }
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
                    DisplayName = "Offline access",
                    Emphasize = true
                };
            }
        }
    }
}