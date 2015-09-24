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
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer3.Core.Logging
{
    internal class AuthorizeRequestValidationLog
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string RedirectUri { get; set; }
        public IEnumerable<string> AllowedRedirectUris { get; set; }
        public string SubjectId { get; set; }

        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
        public Flows Flow { get; set; }
        public string RequestedScopes { get; set; }
        
        public string State { get; set; }
        public string UiLocales { get; set; }
        public string Nonce { get; set; }
        public IEnumerable<string> AuthenticationContextReferenceClasses { get; set; }
        public string DisplayMode { get; set; }
        public string PromptMode { get; set; }
        public int? MaxAge { get; set; }
        public string LoginHint { get; set; }
        public string SessionId { get; set; }

        public Dictionary<string, string> Raw { get; set; }

        public AuthorizeRequestValidationLog(ValidatedAuthorizeRequest request)
        {
            Raw = request.Raw.ToDictionary();

            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;

                AllowedRedirectUris = request.Client.RedirectUris;
            }

            if (request.Subject != null)
            {
                var subjectClaim = request.Subject.FindFirst(Constants.ClaimTypes.Subject);
                if (subjectClaim != null)
                {
                    SubjectId = subjectClaim.Value;
                }
                else
                {
                    SubjectId = "unknown";
                }
            }

            if (request.AuthenticationContextReferenceClasses.Any())
            {
                AuthenticationContextReferenceClasses = request.AuthenticationContextReferenceClasses;
            }
                
            RedirectUri = request.RedirectUri;
            ResponseType = request.ResponseType;
            ResponseMode = request.ResponseMode;
            Flow = request.Flow;
            RequestedScopes = request.RequestedScopes.ToSpaceSeparatedString();
            State = request.State;
            UiLocales = request.UiLocales;
            Nonce = request.Nonce;
            
            DisplayMode = request.DisplayMode;
            PromptMode = request.PromptMode;
            LoginHint = request.LoginHint;
            MaxAge = request.MaxAge;
            SessionId = request.SessionId;
        }
    }
}