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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// AuthenticateResult models the result from the various authentication methods 
    /// on the <see cref="IdentityServer3.Core.Services.IUserService"/>
    /// </summary>
    public class AuthenticateResult
    {
        /// <summary>
        /// The user created from the authentication.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        public ClaimsPrincipal User { get; private set; }

        /// <summary>
        /// The error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// The partial sign in redirect path.
        /// </summary>
        /// <value>
        /// The partial sign in redirect path.
        /// </value>
        public string PartialSignInRedirectPath { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateResult"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="System.ArgumentNullException">errorMessage</exception>
        public AuthenticateResult(string errorMessage)
        {
            if (errorMessage.IsMissing()) throw new ArgumentNullException("errorMessage");
            ErrorMessage = errorMessage;
        }
        
        internal AuthenticateResult(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException("user");
            this.User = IdentityServerPrincipal.CreateFromPrincipal(user, Constants.PrimaryAuthenticationType);
        }

        void Init(string subject, string name,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider,
            string authenticationMethod = null,
            string authenticationType = Constants.PrimaryAuthenticationType
        )
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (String.IsNullOrWhiteSpace(identityProvider)) throw new ArgumentNullException("identityProvider");

            if (String.IsNullOrWhiteSpace(authenticationMethod))
            {
                if (identityProvider == Constants.BuiltInIdentityProvider)
                {
                    authenticationMethod = Constants.AuthenticationMethods.Password;
                }
                else
                {
                    authenticationMethod = Constants.AuthenticationMethods.External;
                }
            }

            var user = IdentityServerPrincipal.Create(subject, name, authenticationMethod, identityProvider, authenticationType);
            if (claims != null && claims.Any())
            {
                claims = claims.Where(x => !Constants.OidcProtocolClaimTypes.Contains(x.Type));
                claims = claims.Where(x => x.Type != Constants.ClaimTypes.Name);
                user.Identities.First().AddClaims(claims);
            }

            this.User = user;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateResult"/> class. This
        /// version of the constructor indicates a full login.
        /// </summary>
        /// <param name="subject">The subject claim used to uniquely identifier the user.</param>
        /// <param name="name">The name claim used as the display name.</param>
        /// <param name="claims">Additional claims that will be maintained in the principal.</param>
        /// <param name="identityProvider">The identity provider. This should used when an external 
        /// identity provider is used and will typically match the <c>AuthenticationType</c> as configured
        /// on the Katana authentication middleware.</param>
        /// <param name="authenticationMethod">The authentication method. This should be used when 
        /// local authentication is performed as some other means other than password has been 
        /// used to authenticate the user (e.g. '2fa' for two-factor, or 'certificate' for client 
        /// certificates).
        /// </param>
        public AuthenticateResult(string subject, string name,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider,
            string authenticationMethod = null
        )
        {
            Init(subject, name, claims, identityProvider, authenticationMethod);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateResult" /> class. This
        /// version of the constructor indicates a partial login (with a redirect) with
        /// knowledge of the subject claim.
        /// </summary>
        /// <param name="redirectPath">The redirect path. This should be relative to the 
        /// current web server. The <c>"~/"</c> prefix is supported to allow application-relative
        /// paths to be used (e.g. "~/path").
        /// </param>
        /// <param name="subject">The subject claim used to uniquely identifier the user.</param>
        /// <param name="name">The name claim used as the display name.</param>
        /// <param name="claims">Additional claims that will be maintained in the principal.</param>
        /// <param name="identityProvider">The identity provider. This should used when an external 
        /// identity provider is used and will typically match the <c>AuthenticationType</c> as configured
        /// on the Katana authentication middleware.</param>
        /// <param name="authenticationMethod">The authentication method. This should be used when
        /// local authentication is performed as some other means other than password has been
        /// used to authenticate the user (e.g. '2fa' for two-factor, or 'certificate' for client</param>
        /// <exception cref="System.ArgumentNullException">redirectPath</exception>
        /// <exception cref="System.ArgumentException">redirectPath must start with / or ~/</exception>
        public AuthenticateResult(string redirectPath, string subject, string name, 
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.BuiltInIdentityProvider,
            string authenticationMethod = null
        )
        {
            if (redirectPath.IsMissing()) throw new ArgumentNullException("redirectPath");
            if (!redirectPath.StartsWith("~/") && !redirectPath.StartsWith("/"))
            {
                throw new ArgumentException("redirectPath must start with / or ~/");
            }

            Init(subject, name, claims, identityProvider, authenticationMethod, Constants.PartialSignInAuthenticationType);
            this.PartialSignInRedirectPath = redirectPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateResult" /> class. This
        /// version of the constructor indicates a partial login (with a redirect) without
        /// knowledge of the subject claim.
        /// </summary>
        /// <param name="redirectPath">The redirect path. This should be relative to the 
        /// current web server. The <c>"~/"</c> prefix is supported to allow application-relative
        /// paths to be used (e.g. "~/path").
        /// </param>
        /// <param name="externalId">The external identifier that represents the external identity
        /// provider the partial login is created from. This will be re-presented to correlate the request
        /// when the user resumes from the redirect.</param>
        /// <exception cref="System.ArgumentNullException">
        /// redirectPath
        /// or
        /// externalId
        /// </exception>
        /// <exception cref="System.ArgumentException">redirectPath must start with / or ~/</exception>
        public AuthenticateResult(string redirectPath, ExternalIdentity externalId)
        {
            if (redirectPath.IsMissing()) throw new ArgumentNullException("redirectPath");
            if (!redirectPath.StartsWith("~/") && !redirectPath.StartsWith("/"))
            {
                throw new ArgumentException("redirectPath must start with / or ~/");
            }
            if (externalId == null) throw new ArgumentNullException("externalId");

            this.PartialSignInRedirectPath = redirectPath;

            var id = new ClaimsIdentity(externalId.Claims, Constants.PartialSignInAuthenticationType);
            // we're keeping the external provider info for the partial signin so we can re-execute AuthenticateExternalAsync
            // once the user is re-directed back into identityserver from the external redirect
            id.AddClaim(new Claim(Constants.ClaimTypes.ExternalProviderUserId, externalId.ProviderId, ClaimValueTypes.String, externalId.Provider));
            User = new ClaimsPrincipal(id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateResult" /> class. This
        /// version of the constructor indicates a partial login (with a redirect) without
        /// knowledge of the subject claim.
        /// </summary>
        /// <param name="redirectPath">The redirect path. This should be relative to the
        /// current web server. The <c>"~/"</c> prefix is supported to allow application-relative
        /// paths to be used (e.g. "~/path").</param>
        /// <param name="claims">Additional claims that will be maintained in the principal.</param>
        /// <exception cref="System.ArgumentNullException">redirectPath</exception>
        /// <exception cref="System.ArgumentException">redirectPath must start with / or ~/</exception>
        public AuthenticateResult(string redirectPath, IEnumerable<Claim> claims)
        {
            if (redirectPath.IsMissing()) throw new ArgumentNullException("redirectPath");
            if (!redirectPath.StartsWith("~/") && !redirectPath.StartsWith("/"))
            {
                throw new ArgumentException("redirectPath must start with / or ~/");
            }

            if (claims == null)
            {
                claims = Enumerable.Empty<Claim>();
            }

            var id = new ClaimsIdentity(claims, Constants.PartialSignInAuthenticationType, Constants.ClaimTypes.Name, Constants.ClaimTypes.Role);
            this.User = new ClaimsPrincipal(id); 
            
            this.PartialSignInRedirectPath = redirectPath;
        }

        /// <summary>
        /// Gets a value indicating whether the authentication resulted in an error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </value>
        public bool IsError
        {
            get { return ErrorMessage.IsPresent(); }
        }

        /// <summary>
        /// Gets a value indicating whether the authentication resulted in a partial sign in.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is partial sign in; otherwise, <c>false</c>.
        /// </value>
        public bool IsPartialSignIn
        {
            get
            {
                return !String.IsNullOrWhiteSpace(PartialSignInRedirectPath);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the authentication result has a subject claim.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has a subject claim; otherwise, <c>false</c>.
        /// </value>
        public bool HasSubject
        {
            get
            {
                return User != null && User.HasClaim(c => c.Type == Constants.ClaimTypes.Subject);
            }
        }
    }
}