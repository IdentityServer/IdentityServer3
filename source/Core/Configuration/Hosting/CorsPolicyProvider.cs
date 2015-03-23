﻿/*
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
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration.Hosting
{
    internal class CorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        
        readonly string[] _paths;

        public CorsPolicyProvider(IEnumerable<string> allowedPaths)
        {
            if (allowedPaths == null) throw new ArgumentNullException("allowedPaths");

            _paths = allowedPaths.Select(Normalize).ToArray();
        }

        public async Task<System.Web.Cors.CorsPolicy> GetCorsPolicyAsync(IOwinRequest request)
        {
            var path = request.Path.ToString();
            var origin = request.Headers["Origin"];
            
            if (IsPathAllowed(request) && origin != null)
            {
                Logger.InfoFormat("CORS request made for path: {0} from origin: {1}", path, origin);

                if (await IsOriginAllowed(origin, request.Environment))
                {
                    Logger.Info("CorsPolicyService allowed origin");
                    return Allow(origin);
                }
                Logger.Info("CorsPolicyService did not allow origin");
            }
            else
            {
                Logger.WarnFormat("CORS request made for path: {0} from origin: {1} but rejected because invalid CORS path", path, origin);
            }

            return null;
        }

        protected virtual async Task<bool> IsOriginAllowed(string origin, IDictionary<string, object> env)
        {
            var corsPolicy = env.ResolveDependency<ICorsPolicyService>();
            return await corsPolicy.IsOriginAllowedAsync(origin);
        }

        private bool IsPathAllowed(IOwinRequest request)
        {
            var requestPath = Normalize(request.Path.Value);
            return _paths.Any(path => requestPath.Equals(path, StringComparison.OrdinalIgnoreCase));
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
            var p = new System.Web.Cors.CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
            };

            p.Origins.Add(origin);
            return p;
        }
    }
}
