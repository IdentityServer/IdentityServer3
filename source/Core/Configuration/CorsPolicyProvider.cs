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

using Microsoft.Owin;
using Microsoft.Owin.Cors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    class CorsPolicyProvider : ICorsPolicyProvider
    {
        readonly CorsPolicy policy;
        readonly string[] paths;

        public CorsPolicyProvider(CorsPolicy policy, IEnumerable<string> allowedPaths)
        {
            if (policy == null) throw new ArgumentNullException("policy");
            if (allowedPaths == null) throw new ArgumentNullException("allowedPaths");

            this.policy = policy;
            this.paths = allowedPaths.Select(path => Normalize(path)).ToArray();
        }

        public async Task<System.Web.Cors.CorsPolicy> GetCorsPolicyAsync(IOwinRequest request)
        {
            if (IsPathAllowed(request))
            {
                var origin = request.Headers["Origin"];
                if (origin != null)
                {
                    if (policy.AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                    {
                        return Allow(origin);
                    }

                    if (policy.PolicyCallback != null)
                    {
                        if (await policy.PolicyCallback(origin))
                        {
                            return Allow(origin);
                        }
                    }
                }
            }
            return null;
        }

        private bool IsPathAllowed(IOwinRequest request)
        {
            var requestPath = Normalize(request.Path.Value);
            return paths.Any(path => requestPath.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        private string Normalize(string path)
        {
            if (String.IsNullOrWhiteSpace(path) || path == "/")
            {
                path = "/";
            }
            else
            {
                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                if (path.EndsWith("/"))
                {
                    path = path.Substring(0, path.Length - 1);
                }
            }
            
            return path;
        }

        System.Web.Cors.CorsPolicy Allow(string origin)
        {
            var policy = new System.Web.Cors.CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
            };
            policy.Origins.Add(origin);
            return policy;
        }
    }
}
