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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IdentityServer3.Core.Extensions
{
    internal static class ScopeExtensions
    {
        [DebuggerStepThrough]
        public static string ToSpaceSeparatedString(this IEnumerable<Scope> scopes)
        {
            var scopeNames = from s in scopes
                             select s.Name;

            return string.Join(" ", scopeNames.ToArray());
        }

        [DebuggerStepThrough]
        public static IEnumerable<string> ToStringList(this IEnumerable<Scope> scopes)
        {
            var scopeNames = from s in scopes
                             select s.Name;

            return scopeNames;
        }

        [DebuggerStepThrough]
        public static bool IncludesAllClaimsForUserRule(this IEnumerable<Scope> scopes, ScopeType type)
        {
            foreach (var scope in scopes)
            {
                if (scope.Type == type)
                {
                    if (scope.IncludeAllClaimsForUser)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}