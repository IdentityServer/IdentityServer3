﻿using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Host.Config
{
    static class Users
    {
        public static IEnumerable<InMemoryUser> Get()
        {
            var users = new InMemoryUser[]
            {
                new InMemoryUser{Subject = "818727", Username = "alice", Password = "alice", 
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.Name, "Alice Smith"),
                        new Claim(Constants.ClaimTypes.GivenName, "Alice"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "AliceSmith@email.com"),
                        new Claim(Constants.ClaimTypes.Role, "Admin"),
                        new Claim(Constants.ClaimTypes.Role, "Geek")
                    }
                },
                new InMemoryUser{Subject = "88421113", Username = "bob", Password = "bob", 
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.Name, "Bob Smith"),
                        new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(Constants.ClaimTypes.Role, "Developer"),
                        new Claim(Constants.ClaimTypes.Role, "Geek")
                    }
                },
            };

            return users;
        }
    }
}