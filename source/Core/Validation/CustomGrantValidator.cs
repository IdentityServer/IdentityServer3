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

using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    internal class CustomGrantValidator
    {
        private readonly IEnumerable<ICustomGrantValidator> _validators;
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public CustomGrantValidator(IEnumerable<ICustomGrantValidator> validators)
        {
            if (validators == null)
            {
                _validators = Enumerable.Empty<ICustomGrantValidator>();
            }
            else
            {
                _validators = validators;
            }
        }

        public IEnumerable<string> GetAvailableGrantTypes()
        {
            return _validators.Select(v => v.GrantType);
        }

        public async Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            var validator = _validators.FirstOrDefault(v => v.GrantType.Equals(request.GrantType, StringComparison.Ordinal));

            if (validator == null)
            {
                return new CustomGrantValidationResult
                {
                    IsError = true,
                    Error = "No validator found for grant type"
                };
            }

            try
            {
                return await validator.ValidateAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error("Grant validation error:" + e.Message);
                return new CustomGrantValidationResult
                {
                    IsError = true,
                    Error = "Grant validation error",
                };
            }
        }
    }
}