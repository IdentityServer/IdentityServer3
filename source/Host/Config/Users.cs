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
using System.Security.Claims;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Host.Config
{
    static class Users
    {
        public static List<InMemoryUser> Get()
        {
            var users = new List<InMemoryUser>
            {
                new InMemoryUser{Subject = "818727", Username = "alice", Password = "alice", 
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.NAME, "Alice Smith"),
                        new Claim(Constants.ClaimTypes.GIVEN_NAME, "Alice"),
                        new Claim(Constants.ClaimTypes.FAMILY_NAME, "Smith"),
                        new Claim(Constants.ClaimTypes.EMAIL, "AliceSmith@email.com"),
                        new Claim(Constants.ClaimTypes.EMAIL_VERIFIED, "true", ClaimValueTypes.Boolean),
                        new Claim(Constants.ClaimTypes.ROLE, "Admin"),
                        new Claim(Constants.ClaimTypes.ROLE, "Geek"),
                        new Claim(Constants.ClaimTypes.WEB_SITE, "http://alice.com"),
                        new Claim(Constants.ClaimTypes.ADDRESS, "{ \"street_address\": \"One Hacker Way\", \"locality\": \"Heidelberg\", \"postal_code\": 69118, \"country\": \"Germany\" }")
                    }
                },
                new InMemoryUser{Subject = "88421113", Username = "bob", Password = "bob", 
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.NAME, "Bob Smith"),
                        new Claim(Constants.ClaimTypes.GIVEN_NAME, "Bob"),
                        new Claim(Constants.ClaimTypes.FAMILY_NAME, "Smith"),
                        new Claim(Constants.ClaimTypes.EMAIL, "BobSmith@email.com"),
                        new Claim(Constants.ClaimTypes.EMAIL_VERIFIED, "true", ClaimValueTypes.Boolean),
                        new Claim(Constants.ClaimTypes.ROLE, "Developer"),
                        new Claim(Constants.ClaimTypes.ROLE, "Geek"),
                        new Claim(Constants.ClaimTypes.WEB_SITE, "http://bob.com"),
                        new Claim(Constants.ClaimTypes.ADDRESS, "{ \"street_address\": \"One Hacker Way\", \"locality\": \"Heidelberg\", \"postal_code\": 69118, \"country\": \"Germany\" }")
                    }
                },
            };

            return users;
        }
    }
}