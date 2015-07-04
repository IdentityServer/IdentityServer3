using IdentityServer3.Core.Models;
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Extensions;
using IdentityModel;
using IdentityServer3.Core.Validation;

namespace IdentityServer3.Core.Services.Default
{
    class PlainTextSharedSecretValidator : ISecretValidator
    {
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });
            var success = Task.FromResult(new SecretValidationResult { Success = true });

            if (parsedSecret.Type == Constants.ParsedSecretTypes.SharedSecret)
            {
                var sharedSecret = parsedSecret.Credential as string;

                if (parsedSecret.Id.IsMissing() || sharedSecret.IsMissing())
                {
                    throw new ArgumentException("id or credential is missing.");
                }

                foreach (var secret in secrets)
                {
                    // this validator is only applicable to shared secrets
                    if (secret.Type != Constants.SecretTypes.SharedSecret)
                    {
                        continue;
                    }

                    // check if client secret is still valid
                    if (secret.Expiration.HasExpired()) continue;

                    // use time constant string comparison
                    var isValid = TimeConstantComparer.IsEqual(sharedSecret, secret.Value);

                    if (isValid)
                    {
                        return success;
                    }
                }
            }

            return fail;
        }
    }
}
