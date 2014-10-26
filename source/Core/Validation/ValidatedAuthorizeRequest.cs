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
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class ValidatedAuthorizeRequest : ValidatedRequest
    {
        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
        public Flows Flow { get; set; }
        
        public Client Client { get; set; }
        public Uri RedirectUri { get; set; }

        public string ClientId { get; set; }
        public List<string> RequestedScopes { get; set; }
        public bool WasConsentShown { get; set; }
        public string State { get; set; }
        public string UiLocales { get; set; }
        
        public bool IsOpenIdRequest { get; set; }
        public bool IsResourceRequest { get; set; }
        
        public string Nonce { get; set; }
        public List<string> AuthenticationContextReferenceClasses { get; set; }
        public string DisplayMode { get; set; }
        public string PromptMode { get; set; }
        public int? MaxAge { get; set; }
        public string LoginHint { get; set; }

        public bool AccessTokenRequested
        {
            get
            {
                return (ResponseType == Constants.ResponseTypes.IdTokenToken ||
                        ResponseType == Constants.ResponseTypes.Code ||
                        ResponseType == Constants.ResponseTypes.CodeToken ||
                        ResponseType == Constants.ResponseTypes.CodeIdTokenToken);
            }
        }

        public ValidatedAuthorizeRequest()
        {
            RequestedScopes = new List<string>();
            AuthenticationContextReferenceClasses = new List<string>();
        }
    }
}