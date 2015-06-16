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

using System.ComponentModel;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Results;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class WelcomeController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public WelcomeController()
        {
        }

        public IHttpActionResult Get()
        {
            Logger.Info("Welcome page requested - rendering");
            return new WelcomeActionResult(Request.GetOwinContext());
        }
    }
}