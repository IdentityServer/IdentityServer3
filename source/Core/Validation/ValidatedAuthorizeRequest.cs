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

namespace IdentityServer3.Core.Validation
{
    /// <summary>
    /// Models a validated request to the authorize endpoint.
    /// </summary>
    public class ValidatedAuthorizeRequest : ValidatedRequest
    {
        /// <summary>
        /// Gets or sets the type of the response.
        /// </summary>
        /// <value>
        /// The type of the response.
        /// </value>
        public string ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the response mode.
        /// </summary>
        /// <value>
        /// The response mode.
        /// </value>
        public string ResponseMode { get; set; }

        /// <summary>
        /// Gets or sets the flow.
        /// </summary>
        /// <value>
        /// The flow.
        /// </value>
        public Flows Flow { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the requested scopes.
        /// </summary>
        /// <value>
        /// The requested scopes.
        /// </value>
        public List<string> RequestedScopes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether consent was shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent was shown; otherwise, <c>false</c>.
        /// </value>
        public bool WasConsentShown { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the UI locales.
        /// </summary>
        /// <value>
        /// The UI locales.
        /// </value>
        public string UiLocales { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request was an OpenID Connect request.
        /// </summary>
        /// <value>
        /// <c>true</c> if the request was an OpenID Connect request; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpenIdRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is resource request.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is resource request; otherwise, <c>false</c>.
        /// </value>
        public bool IsResourceRequest { get; set; }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        /// <value>
        /// The nonce.
        /// </value>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets the authentication context reference classes.
        /// </summary>
        /// <value>
        /// The authentication context reference classes.
        /// </value>
        public List<string> AuthenticationContextReferenceClasses { get; set; }

        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public string DisplayMode { get; set; }

        /// <summary>
        /// Gets or sets the prompt mode.
        /// </summary>
        /// <value>
        /// The prompt mode.
        /// </value>
        public string PromptMode { get; set; }

        /// <summary>
        /// Gets or sets the maximum age.
        /// </summary>
        /// <value>
        /// The maximum age.
        /// </value>
        public int? MaxAge { get; set; }

        /// <summary>
        /// Gets or sets the login hint.
        /// </summary>
        /// <value>
        /// The login hint.
        /// </value>
        public string LoginHint { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets a value indicating whether an access token was requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if an access token was requested; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatedAuthorizeRequest"/> class.
        /// </summary>
        public ValidatedAuthorizeRequest()
        {
            RequestedScopes = new List<string>();
            AuthenticationContextReferenceClasses = new List<string>();
        }
    }
}