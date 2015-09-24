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

namespace IdentityServer3.Core.Logging
{
    internal class TokenValidationLog
    {
        // identity token
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public bool ValidateLifetime { get; set; }

        // access token
        public string AccessTokenType { get; set; }
        public string ExpectedScope { get; set; }
        public string TokenHandle { get; set; }
        public string JwtId { get; set; }

        // both
        public Dictionary<string, object> Claims { get; set; }
    }
}
