/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    class CorsPolicyProvider : ICorsPolicyProvider
    {
        readonly CorsPolicy policy;
        readonly string[] paths;

        public CorsPolicyProvider(CorsPolicy policy, string[] allowedPaths)
        {
            if (policy == null) throw new ArgumentNullException("policy");
            if (allowedPaths == null) throw new ArgumentNullException("allowedPaths");

            this.policy = policy;
            this.paths = allowedPaths.Select(path => Normalize(path)).ToArray();
        }

        public async Task<System.Web.Cors.CorsPolicy> GetCorsPolicyAsync(Microsoft.Owin.IOwinRequest request)
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
