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

using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Validation;
using System;
using System.Collections.Generic;

namespace IdentityServer3.Core.Configuration
{
    /// <summary>
    /// Use this class to replace built-in services, or add additional dependencies to the container
    /// </summary>
    public class IdentityServerServiceFactory
    {
        static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        static readonly Registration<IExternalClaimsFilter> DefaultClaimsFilter;

        /// <summary>
        /// Initializes the <see cref="IdentityServerServiceFactory"/> class.
        /// </summary>
        static IdentityServerServiceFactory()
        {
            DefaultClaimsFilter = new Registration<IExternalClaimsFilter>(resolver =>
            {
                var aggregateFilter = new AggregateExternalClaimsFilter(
                    new NormalizingClaimsFilter(),
                    new FacebookClaimsFilter(),
                    new TwitterClaimsFilter()
                );

                return aggregateFilter;
            });
        }

        // keep list of any additional dependencies the 
        // hosting application might need. these will be
        // added to the DI container
        readonly List<Registration> _registrations = new List<Registration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerServiceFactory"/> class.
        /// </summary>
        public IdentityServerServiceFactory()
        {
            this.ExternalClaimsFilter = DefaultClaimsFilter;

            CustomGrantValidators = new List<Registration<ICustomGrantValidator>>();

            // register default secret parsers
            SecretParsers = new List<Registration<ISecretParser>>
            {
                new Registration<ISecretParser, BasicAuthenticationSecretParser>(),
                new Registration<ISecretParser, PostBodySecretParser>(),
                new Registration<ISecretParser, X509CertificateSecretParser>(),
            };

            // register default secret validators
            SecretValidators = new List<Registration<ISecretValidator>>
            {
                new Registration<ISecretValidator, HashedSharedSecretValidator>(),
                new Registration<ISecretValidator, X509CertificateThumbprintSecretValidator>()
            };
        }

        /// <summary>
        /// Gets the a list of additional dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        public IEnumerable<Registration> Registrations
        {
            get { return _registrations; }
        }

        /// <summary>
        /// Adds a registration to the dependency list
        /// </summary>
        /// <typeparam name="T">Type of the dependency</typeparam>
        /// <param name="registration">The registration.</param>
        public void Register<T>(Registration<T> registration)
            where T : class
        {
            _registrations.Add(registration);
        }

        ///////////////////////
        // mandatory (external)
        ///////////////////////

        /// <summary>
        /// Gets or sets the user service. The user service implements user authentication against the local user store as well as association of external users. There are standard implementations for in-memory, MembershipReboot and ASP.NET Identity
        /// </summary>
        /// <value>
        /// The user service.
        /// </value>
        public Registration<IUserService> UserService { get; set; }

        /// <summary>
        /// Gets or sets the scope store. The scope store implements retrieval of scopes configuration data.
        /// </summary>
        /// <value>
        /// The scope store.
        /// </value>
        public Registration<IScopeStore> ScopeStore { get; set; }

        /// <summary>
        /// Gets or sets the client store. The client store implements retrieval of client configuration data.
        /// </summary>
        /// <value>
        /// The client store.
        /// </value>
        public Registration<IClientStore> ClientStore { get; set; }


        ///////////////////////
        // mandatory (for authorization code, reference & refresh tokens and consent)
        // but with default in memory implementation
        ///////////////////////

        /// <summary>
        /// Gets or sets the authorization code store - implements storage and retrieval of authorization codes.
        /// </summary>
        /// <value>
        /// The authorization code store.
        /// </value>
        public Registration<IAuthorizationCodeStore> AuthorizationCodeStore { get; set; }

        /// <summary>
        /// Gets or sets the token handle store - Implements storage and retrieval of handles for reference tokens.
        /// </summary>
        /// <value>
        /// The token handle store.
        /// </value>
        public Registration<ITokenHandleStore> TokenHandleStore { get; set; }

        /// <summary>
        /// Gets or sets the consent store - Implements storage and retrieval of consent decisions.
        /// </summary>
        /// <value>
        /// The consent store.
        /// </value>
        public Registration<IConsentStore> ConsentStore { get; set; }

        /// <summary>
        /// Gets or sets the refresh token store - Implements storage and retrieval of refresh tokens.
        /// </summary>
        /// <value>
        /// The refresh token store.
        /// </value>
        public Registration<IRefreshTokenStore> RefreshTokenStore { get; set; }

        /// <summary>
        /// Gets or sets the view service - Implements retrieval of UI assets. Defaults to using the embedded assets.
        /// </summary>
        /// <value>
        /// The view service.
        /// </value>
        public Registration<IViewService> ViewService { get; set; }


        ///////////////////////
        // optional (with default implementations)
        ///////////////////////

        /// <summary>
        /// Gets or sets the consent service - Implements logic of consent decisions 
        /// </summary>
        /// <value>
        /// The consent service.
        /// </value>
        public Registration<IConsentService> ConsentService { get; set; }

        /// <summary>
        /// Gets or sets the client permissions service - Implements retrieval and revocation of consents, reference and refresh tokens.
        /// </summary>
        /// <value>
        /// The client permissions service.
        /// </value>
        public Registration<IClientPermissionsService> ClientPermissionsService { get; set; }

        /// <summary>
        /// Gets or sets the custom grant validator - Implements validation of custom grant types.
        /// </summary>
        /// <value>
        /// The custom grant validator.
        /// </value>
        public List<Registration<ICustomGrantValidator>> CustomGrantValidators { get; set; }

        /// <summary>
        /// Gets or sets the custom request validator - Implements custom additional validation of authorize and token requests.
        /// </summary>
        /// <value>
        /// The custom request validator.
        /// </value>
        public Registration<ICustomRequestValidator> CustomRequestValidator { get; set; }

        /// <summary>
        /// Gets or sets the claims provider - Implements retrieval of claims for identity and access tokens.
        /// </summary>
        /// <value>
        /// The claims provider.
        /// </value>
        public Registration<IClaimsProvider> ClaimsProvider { get; set; }

        /// <summary>
        /// Gets or sets the token service - Implements creation of security tokens definitions.
        /// </summary>
        /// <value>
        /// The token service.
        /// </value>
        public Registration<ITokenService> TokenService { get; set; }

        /// <summary>
        /// Gets or sets the refresh token service - Implements creation and updates of refresh tokens.
        /// </summary>
        /// <value>
        /// The refresh token service.
        /// </value>
        public Registration<IRefreshTokenService> RefreshTokenService { get; set; }

        /// <summary>
        /// Gets or sets the token signing service - Implements creation and signing of security tokens.
        /// </summary>
        /// <value>
        /// The token signing service.
        /// </value>
        public Registration<ITokenSigningService> TokenSigningService { get; set; }

        /// <summary>
        /// Gets or sets the external claims filter - Implements filtering and transformation of claims for external identity providers.
        /// </summary>
        /// <value>
        /// The external claims filter.
        /// </value>
        public Registration<IExternalClaimsFilter> ExternalClaimsFilter { get; set; }

        /// <summary>
        /// Gets or sets the event service.
        /// </summary>
        /// <value>
        /// The event service.
        /// </value>
        public Registration<IEventService> EventService { get; set; }

        /// <summary>
        /// Gets or sets the custom token validator - Implements custom additional validation of tokens for the token validation endpoints.
        /// </summary>
        /// <value>
        /// The custom token validator.
        /// </value>
        public Registration<ICustomTokenValidator> CustomTokenValidator { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI validator.
        /// </summary>
        /// <value>
        /// The redirect URI validator.
        /// </value>
        public Registration<IRedirectUriValidator> RedirectUriValidator { get; set; }
        
        /// <summary>
        /// Gets or sets the localization service.
        /// </summary>
        /// <value>
        /// The localization service.
        /// </value>
        public Registration<ILocalizationService> LocalizationService { get; set; }

        /// <summary>
        /// Gets or sets the secret parsers.
        /// </summary>
        /// <value>
        /// The secret parsers.
        /// </value>
        public IEnumerable<Registration<ISecretParser>> SecretParsers { get; set; }

        /// <summary>
        /// Gets or sets the secret validators.
        /// </summary>
        /// <value>
        /// The secret validators.
        /// </value>
        public IEnumerable<Registration<ISecretValidator>> SecretValidators { get; set; }

        /// <summary>
        /// Gets or sets the CORS policy service.
        /// </summary>
        /// <value>
        /// The CORS policy service.
        /// </value>
        public Registration<ICorsPolicyService> CorsPolicyService { get; set; }

        internal void Validate()
        {
            if (UserService == null) LogAndStop("UserService not configured");
            if (ScopeStore == null) LogAndStop("ScopeStore not configured.");
            if (ClientStore == null) LogAndStop("ClientStore not configured.");

            if (AuthorizationCodeStore == null) Logger.Warn("AuthorizationCodeStore not configured - falling back to InMemory");
            if (TokenHandleStore == null) Logger.Warn("TokenHandleStore not configured - falling back to InMemory");
            if (ConsentStore == null) Logger.Warn("ConsentStore not configured - falling back to InMemory");
            if (RefreshTokenStore == null) Logger.Warn("RefreshTokenStore not configured - falling back to InMemory");
            if (RedirectUriValidator != null) Logger.Warn("Using custom redirect URI validator - you are running with scissors.");
        }

        private void LogAndStop(string message)
        {
            Logger.Error(message);
            throw new InvalidOperationException(message);
        }
    }
}