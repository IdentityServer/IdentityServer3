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

namespace Thinktecture.IdentityServer.Core.Services.InMemory
{
    public class InMemoryUser
    {
        public string Subject { get; set; }
        public bool Enabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Provider { get; set; }
        public string ProviderId { get; set; }
        public IEnumerable<Claim> Claims { get; set; }

        public InMemoryUser()
        {
            Enabled = true;
            Claims = new List<Claim>();
        }
    }
}