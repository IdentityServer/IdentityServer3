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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Core.ResponseHandling
{
    internal class IntrospectionResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult, Scope scope)
        {
            Logger.Info("Creating introspection response");

            var response = new Dictionary<string, object>();
            
            if (validationResult.IsActive == false)
            {
                Logger.Info("Creating introspection response for inactive token.");

                response.Add("active", false);
                return Task.FromResult(response);
            }

            if (scope.AllowUnrestrictedIntrospection)
            {
                Logger.Info("Creating unrestricted introspection response for active token.");

                response = validationResult.Claims.ToClaimsDictionary();
                response.Add("active", true);
            }
            else
            {
                Logger.Info("Creating restricted introspection response for active token.");

                response = validationResult.Claims.Where(c => c.Type != Constants.ClaimTypes.Scope).ToClaimsDictionary();
                response.Add("active", true);
                response.Add("scope", scope.Name);
            }

            return Task.FromResult(response);
        }
    }
}