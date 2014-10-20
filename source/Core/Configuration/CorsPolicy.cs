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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class CorsPolicy
    {
        public CorsPolicy()
        {
            AllowedOrigins = new List<string>();
        }
        
        public ICollection<string> AllowedOrigins { get; private set; }
        public Func<string, Task<bool>> PolicyCallback { get; set; }

        public static readonly CorsPolicy AllowAll = new CorsPolicy
        {
            PolicyCallback = origin => Task.FromResult(true)
        };
    }
}
