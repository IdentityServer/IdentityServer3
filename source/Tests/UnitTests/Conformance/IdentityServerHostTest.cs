using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;
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
using System.Security.Claims;

namespace IdentityServer3.Tests.Conformance
{
    public class IdentityServerHostTest : IDisposable
    {
        protected IdentityServerHost host = new IdentityServerHost();
        
        public IdentityServerHostTest()
        {
            host.Users.Add(new InMemoryUser{
                Subject = "818727", Username = "bob", Password = "bob", 
                Claims = new Claim[]
                {
                    new Claim(Constants.ClaimTypes.Name, "Bob Loblaw"),
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Loblaw"),
                    new Claim(Constants.ClaimTypes.Email, "bob@email.com"),
                    new Claim(Constants.ClaimTypes.Role, "Admin"),
                    new Claim(Constants.ClaimTypes.Role, "Geek"),
                    new Claim(Constants.ClaimTypes.WebSite, "http://bob.com"),
                    new Claim(Constants.ClaimTypes.Address, "{ \"street_address\": \"One Hacker Way\", \"locality\": \"Heidelberg\", \"postal_code\": 69118, \"country\": \"Germany\" }")
                }
            });

            Init();
        }

        protected virtual void Init()
        {
            PreInit();
            host.Init();
            PostInit();
        }

        protected virtual void PreInit()
        {
        }

        protected virtual void PostInit()
        {
        }

        public void Dispose()
        {
            host.Dispose();
        }
    }
}
