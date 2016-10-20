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

using IdentityServer3.Core.Models;

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Modles the validation result of token revocation requests.
    /// </summary>
    public class TokenRevocationRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// The token_type_hint: refresh_token, access_token or custom token type
        /// </summary>
        public string TokenTypeHint { get; set; }

        /// <summary>
        /// The token handle
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The client revoking the token(s)
        /// </summary>
        public Client Client { get; set; }
    }
}