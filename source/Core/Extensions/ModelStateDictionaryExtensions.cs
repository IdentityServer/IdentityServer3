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
using System.Linq;
using System.Web.Http.ModelBinding;

namespace IdentityServer3.Core.Extensions
{
    internal static class ModelStateDictionaryExtensions
    {
        public static IEnumerable<string> GetErrors(this ModelStateDictionary modelState)
        {
            if (modelState == null) return Enumerable.Empty<string>();
            var errors =
                from item in modelState
                where item.Value.Errors.Any()
                from err in item.Value.Errors
                select err.ErrorMessage;
            return errors;
        }

        public static string GetError(this ModelStateDictionary modelState)
        {
            return modelState.GetErrors().FirstOrDefault();
        }

    }
}