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
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class TokenCreationRequest
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public ClaimsPrincipal Subject { get; set; }
        public Client Client { get; set; }
        public IEnumerable<Scope> Scopes { get; set; }
        public ValidatedRequest ValidatedRequest { get; set; }

        public bool IncludeAllIdentityClaims { get; set; }
        public string AccessTokenToHash { get; set; }
        public string AuthorizationCodeToHash { get; set; }

        public void Validate()
        {
            if (Client == null) LogAndStop("client");
            if (Scopes == null) LogAndStop("scopes");
            if (ValidatedRequest == null) LogAndStop("validatedRequest");
        }

        private void LogAndStop(string name)
        {
            Logger.ErrorFormat("{0} is null", name);
            throw new ArgumentNullException(name);
        }
    }
}