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

using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer3.Tests.Endpoints
{
    public class TestUsers
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
                {
                    new InMemoryUser{Subject = "818727", Username = "alice", Password = "alice", 
                        Claims = new Claim[]
                        {
                            new Claim(Constants.ClaimTypes.GivenName, "Alice"),
                            new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                            new Claim(Constants.ClaimTypes.Email, "AliceSmith@email.com"),
                        }, 
                        Provider = "Google", 
                        ProviderId = "123"
                    },
                    new InMemoryUser{Subject = "88421113", Username = "bob", Password = "bob", 
                        Claims = new Claim[]
                        {
                            new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                            new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                            new Claim(Constants.ClaimTypes.Email, "BobSmith@email.com"),
                        }
                    },
                };
        }
    }
}
