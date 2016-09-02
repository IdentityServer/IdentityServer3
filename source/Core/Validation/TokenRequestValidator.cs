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

using IdentityModel;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Validation
{
    internal class TokenRequestValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions _options;
        private readonly IAuthorizationCodeStore _authorizationCodes;
        private readonly IUserService _users;
        private readonly CustomGrantValidator _customGrantValidator;
        private readonly ICustomRequestValidator _customRequestValidator;
        private readonly IRefreshTokenStore _refreshTokens;
        private readonly ScopeValidator _scopeValidator;
        private readonly IEventService _events;

        private ValidatedTokenRequest _validatedRequest;

        public ValidatedTokenRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public TokenRequestValidator(IdentityServerOptions options, IAuthorizationCodeStore authorizationCodes, IRefreshTokenStore refreshTokens, IUserService users, CustomGrantValidator customGrantValidator, ICustomRequestValidator customRequestValidator, ScopeValidator scopeValidator, IEventService events)
        {
            _options = options;
            _authorizationCodes = authorizationCodes;
            _refreshTokens = refreshTokens;
            _users = users;
            _customGrantValidator = customGrantValidator;
            _customRequestValidator = customRequestValidator;
            _scopeValidator = scopeValidator;
            _events = events;
        }

        public async Task<TokenRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, Client client)
        {
            Logger.Info("Start token request validation");

            _validatedRequest = new ValidatedTokenRequest();

            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            _validatedRequest.Raw = parameters;
            _validatedRequest.Client = client;
            _validatedRequest.Options = _options;

            /////////////////////////////////////////////
            // check grant type
            /////////////////////////////////////////////
            var grantType = parameters.Get(Constants.TokenRequest.GrantType);
            if (grantType.IsMissing())
            {
                LogError("Grant type is missing.");
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            if (grantType.Length > _options.InputLengthRestrictions.GrantType)
            {
                LogError("Grant type is too long.");
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            _validatedRequest.GrantType = grantType;

            // standard grant types
            switch (grantType)
            {
                case Constants.GrantTypes.AuthorizationCode:
                    return await RunValidationAsync(ValidateAuthorizationCodeRequestAsync, parameters);
                case Constants.GrantTypes.ClientCredentials:
                    return await RunValidationAsync(ValidateClientCredentialsRequestAsync, parameters);
                case Constants.GrantTypes.Password:
                    return await RunValidationAsync(ValidateResourceOwnerCredentialRequestAsync, parameters);
                case Constants.GrantTypes.RefreshToken:
                    return await RunValidationAsync(ValidateRefreshTokenRequestAsync, parameters);
            }

            // custom grant type
            var result = await RunValidationAsync(ValidateCustomGrantRequestAsync, parameters);

            if (result.IsError)
            {
                if (result.Error.IsPresent())
                {
                    return result;
                }

                LogError("Unsupported grant_type: " + grantType);
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            return result;
        }

        async Task<TokenRequestValidationResult> RunValidationAsync(Func<NameValueCollection, Task<TokenRequestValidationResult>> validationFunc, NameValueCollection parameters)
        {
            // run standard validation
            var result = await validationFunc(parameters);
            if (result.IsError)
            {
                return result;
            }

            // run custom validation
            var customResult = await _customRequestValidator.ValidateTokenRequestAsync(_validatedRequest);

            if (customResult.IsError)
            {
                var message = "Custom token request validator error";

                if (customResult.Error.IsPresent())
                {
                    message += ": " + customResult.Error;
                }
                else
                {
                    customResult.Error = Constants.TokenErrors.InvalidRequest;
                }

                LogError(message);
                return customResult;
            }

            LogSuccess();
            return customResult;
        }

        private async Task<TokenRequestValidationResult> ValidateAuthorizationCodeRequestAsync(NameValueCollection parameters)
        {
            Logger.Info("Start validation of authorization code token request");

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (Constants.AllowedFlowsForAuthorizationCodeGrantType.Contains(_validatedRequest.Client.Flow) == false)
            {
                LogError("Client not authorized for code flow");
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // validate authorization code
            /////////////////////////////////////////////
            var code = parameters.Get(Constants.TokenRequest.Code);
            if (code.IsMissing())
            {
                var error = "Authorization code is missing.";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(null, error);

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            if (code.Length > _options.InputLengthRestrictions.AuthorizationCode)
            {
                var error = "Authorization code is too long.";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(null, error);

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.AuthorizationCodeHandle = code;

            var authZcode = await _authorizationCodes.GetAsync(code);
            if (authZcode == null)
            {
                LogError("Invalid authorization code: " + code);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, "Invalid handle");

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            await _authorizationCodes.RemoveAsync(code);

            /////////////////////////////////////////////
            // populate session id
            /////////////////////////////////////////////
            if (authZcode.SessionId.IsPresent())
            {
                _validatedRequest.SessionId = authZcode.SessionId;
            }

            /////////////////////////////////////////////
            // validate client binding
            /////////////////////////////////////////////
            if (authZcode.Client.ClientId != _validatedRequest.Client.ClientId)
            {
                LogError(string.Format("Client {0} is trying to use a code from client {1}", _validatedRequest.Client.ClientId, authZcode.Client.ClientId));
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, "Invalid client binding");

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // validate PKCE parameters
            /////////////////////////////////////////////
            var codeVerifier = parameters.Get(Constants.TokenRequest.CodeVerifier);
            if (authZcode.Client.Flow == Flows.AuthorizationCodeWithProofKey ||
                authZcode.Client.Flow == Flows.HybridWithProofKey)
            {
                var proofKeyResult = ValidateAuthorizationCodeWithProofKeyParameters(codeVerifier, authZcode);
                if (proofKeyResult.IsError)
                {
                    return proofKeyResult;
                }

                _validatedRequest.CodeVerifier = codeVerifier;
            }
            else
            {
                if (codeVerifier.IsPresent())
                {
                    LogError("Unexpected code_verifier with Flow " + authZcode.Client.Flow.ToString());
                    return Invalid(Constants.TokenErrors.InvalidGrant);
                }
            }

            /////////////////////////////////////////////
            // validate code expiration
            /////////////////////////////////////////////
            if (authZcode.CreationTime.HasExceeded(_validatedRequest.Client.AuthorizationCodeLifetime))
            {
                var error = "Authorization code is expired";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.AuthorizationCode = authZcode;

            /////////////////////////////////////////////
            // validate redirect_uri
            /////////////////////////////////////////////
            var redirectUri = parameters.Get(Constants.TokenRequest.RedirectUri);
            if (redirectUri.IsMissing())
            {
                var error = "Redirect URI is missing.";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            if (redirectUri.Equals(_validatedRequest.AuthorizationCode.RedirectUri, StringComparison.Ordinal) == false)
            {
                var error = "Invalid redirect_uri: " + redirectUri;
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // validate scopes are present
            /////////////////////////////////////////////
            if (_validatedRequest.AuthorizationCode.RequestedScopes == null ||
                !_validatedRequest.AuthorizationCode.RequestedScopes.Any())
            {
                var error = "Authorization code has no associated scopes.";
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(Constants.TokenErrors.InvalidRequest);
            }


            /////////////////////////////////////////////
            // make sure user is enabled
            /////////////////////////////////////////////
            var isActiveCtx = new IsActiveContext(_validatedRequest.AuthorizationCode.Subject, _validatedRequest.Client);
            await _users.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                var error = "User has been disabled: " + _validatedRequest.AuthorizationCode.Subject;
                LogError(error);
                await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                return Invalid(Constants.TokenErrors.InvalidRequest);
            }

            /////////////////////////////////////////////
            // validate token type and PoP parameters if pop token is requested
            /////////////////////////////////////////////
            var tokenType = parameters.Get("token_type");
            if (tokenType != null && tokenType == Constants.ResponseTokenTypes.PoP)
            {
                var result = ValidatePopParameters(parameters);
                if (result.IsError)
                {
                    var error = "PoP parameter validation failed: " + result.ErrorDescription;
                    LogError(error);
                    await RaiseFailedAuthorizationCodeRedeemedEventAsync(code, error);

                    return Invalid(result.Error, result.ErrorDescription);
                }
                else
                {
                    _validatedRequest.RequestedTokenType = RequestedTokenTypes.PoP;
                }
            }

            Logger.Info("Validation of authorization code token request success");
            await RaiseSuccessfulAuthorizationCodeRedeemedEventAsync();

            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateClientCredentialsRequestAsync(NameValueCollection parameters)
        {
            Logger.Info("Start client credentials token request validation");

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.ClientCredentials)
            {
                if (_validatedRequest.Client.AllowClientCredentialsOnly == false)
                {
                    LogError("Client not authorized for client credentials flow");
                    return Invalid(Constants.TokenErrors.UnauthorizedClient);
                }
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!(await ValidateRequestedScopesAsync(parameters)))
            {
                LogError("Invalid scopes.");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            if (_validatedRequest.ValidatedScopes.ContainsOpenIdScopes)
            {
                LogError("Client cannot request OpenID scopes in client credentials flow");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            if (_validatedRequest.ValidatedScopes.ContainsOfflineAccessScope)
            {
                LogError("Client cannot request a refresh token in client credentials flow");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            Logger.Info("Client credentials token request validation success");
            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateResourceOwnerCredentialRequestAsync(NameValueCollection parameters)
        {
            Logger.Info("Start password token request validation");

            // if we've disabled local authentication, then fail
            if (_options.AuthenticationOptions.EnableLocalLogin == false ||
                _validatedRequest.Client.EnableLocalLogin == false)
            {
                LogError("EnableLocalLogin is disabled, failing with UnsupportedGrantType");
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.ResourceOwner)
            {
                LogError("Client not authorized for resource owner flow");
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!(await ValidateRequestedScopesAsync(parameters)))
            {
                LogError("Invalid scopes.");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            /////////////////////////////////////////////
            // check resource owner credentials
            /////////////////////////////////////////////
            var userName = parameters.Get(Constants.TokenRequest.UserName);
            var password = parameters.Get(Constants.TokenRequest.Password);

            if (userName.IsMissing() || password.IsMissing())
            {
                LogError("Username or password missing.");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            if (userName.Length > _options.InputLengthRestrictions.UserName ||
                password.Length > _options.InputLengthRestrictions.Password)
            {
                LogError("Username or password too long.");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.UserName = userName;

            /////////////////////////////////////////////
            // check optional parameters and populate SignInMessage
            /////////////////////////////////////////////
            var signInMessage = new SignInMessage();

            // pass through client_id
            signInMessage.ClientId = _validatedRequest.Client.ClientId;

            // process acr values
            var acr = parameters.Get(Constants.AuthorizeRequest.AcrValues);
            if (acr.IsPresent())
            {
                if (acr.Length > _options.InputLengthRestrictions.AcrValues)
                {
                    LogError("Acr values too long.");
                    return Invalid(Constants.TokenErrors.InvalidRequest);
                }

                var acrValues = acr.FromSpaceSeparatedString().Distinct().ToList();

                // look for well-known acr value -- idp
                var idp = acrValues.FirstOrDefault(x => x.StartsWith(Constants.KnownAcrValues.HomeRealm));
                if (idp.IsPresent())
                {
                    signInMessage.IdP = idp.Substring(Constants.KnownAcrValues.HomeRealm.Length);
                    acrValues.Remove(idp);
                }

                // look for well-known acr value -- tenant
                var tenant = acrValues.FirstOrDefault(x => x.StartsWith(Constants.KnownAcrValues.Tenant));
                if (tenant.IsPresent())
                {
                    signInMessage.Tenant = tenant.Substring(Constants.KnownAcrValues.Tenant.Length);
                    acrValues.Remove(tenant);
                }

                // pass through any remaining acr values
                if (acrValues.Any())
                {
                    signInMessage.AcrValues = acrValues;
                }
            }

            _validatedRequest.SignInMessage = signInMessage;

            /////////////////////////////////////////////
            // authenticate user
            /////////////////////////////////////////////
            var authenticationContext = new LocalAuthenticationContext
            {
                UserName = userName,
                Password = password,
                SignInMessage = signInMessage
            };

            await _users.AuthenticateLocalAsync(authenticationContext);
            var authnResult = authenticationContext.AuthenticateResult;

            if (authnResult == null || authnResult.IsError || authnResult.IsPartialSignIn)
            {
                var error = Resources.Messages.InvalidUsernameOrPassword;
                if (authnResult != null && authnResult.IsError)
                {
                    error = authnResult.ErrorMessage;
                }
                if (authnResult != null && authnResult.IsPartialSignIn)
                {
                    error = "Partial signin returned from AuthenticateLocalAsync";
                }
                LogError("User authentication failed: " + error);
                await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, signInMessage, error);

                if (authnResult != null)
                {
                    return Invalid(Constants.TokenErrors.InvalidGrant, authnResult.ErrorMessage);
                }

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.UserName = userName;
            _validatedRequest.Subject = authnResult.User;

            await RaiseSuccessfulResourceOwnerAuthenticationEventAsync(userName, authnResult.User.GetSubjectId(), signInMessage);
            Logger.Info("Password token request validation success.");
            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateRefreshTokenRequestAsync(NameValueCollection parameters)
        {
            Logger.Info("Start validation of refresh token request");

            var refreshTokenHandle = parameters.Get(Constants.TokenRequest.RefreshToken);
            if (refreshTokenHandle.IsMissing())
            {
                var error = "Refresh token is missing";
                LogError(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(null, error);

                return Invalid(Constants.TokenErrors.InvalidRequest);
            }

            if (refreshTokenHandle.Length > _options.InputLengthRestrictions.RefreshToken)
            {
                var error = "Refresh token too long";
                LogError(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(null, error);

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.RefreshTokenHandle = refreshTokenHandle;

            /////////////////////////////////////////////
            // check if refresh token is valid
            /////////////////////////////////////////////
            var refreshToken = await _refreshTokens.GetAsync(refreshTokenHandle);
            if (refreshToken == null)
            {
                var error = "Refresh token is invalid";
                LogWarn(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // check if refresh token has expired
            /////////////////////////////////////////////
            if (refreshToken.CreationTime.HasExceeded(refreshToken.LifeTime))
            {
                var error = "Refresh token has expired";
                LogWarn(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                await _refreshTokens.RemoveAsync(refreshTokenHandle);
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // check if client belongs to requested refresh token
            /////////////////////////////////////////////
            if (_validatedRequest.Client.ClientId != refreshToken.ClientId)
            {
                LogError(string.Format("Client {0} tries to refresh token belonging to client {1}", _validatedRequest.Client.ClientId, refreshToken.ClientId));
                await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, "Invalid client binding");

                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // check if client still has offline_access scope
            /////////////////////////////////////////////
            if (!_validatedRequest.Client.AllowAccessToAllScopes)
            {
                if (!_validatedRequest.Client.AllowedScopes.Contains(Constants.StandardScopes.OfflineAccess))
                {
                    var error = "Client does not have access to offline_access scope anymore";
                    LogError(error);
                    await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                    return Invalid(Constants.TokenErrors.InvalidGrant);
                }
            }

            /////////////////////////////////////////////
            // check if client still has access to 
            // all scopes from the original token request
            /////////////////////////////////////////////
            if (!_validatedRequest.Client.AllowAccessToAllScopes)
            {
                foreach (var scope in refreshToken.Scopes)
                {
                    if (!_validatedRequest.Client.AllowedScopes.Contains(scope))
                    {
                        var error = "Client does not have access to a requested scope anymore: " + scope;
                        LogError(error);
                        await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                        return Invalid(Constants.TokenErrors.InvalidGrant);
                    }
                }
            }

            _validatedRequest.RefreshToken = refreshToken;

            /////////////////////////////////////////////
            // make sure user is enabled
            /////////////////////////////////////////////
            var principal = IdentityServerPrincipal.FromSubjectId(_validatedRequest.RefreshToken.SubjectId, refreshToken.AccessToken.Claims);
            
            var isActiveCtx = new IsActiveContext(principal, _validatedRequest.Client);
            await _users.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                var error = "User has been disabled: " + _validatedRequest.RefreshToken.SubjectId;
                LogError(error);
                await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                return Invalid(Constants.TokenErrors.InvalidRequest);
            }

            /////////////////////////////////////////////
            // validate token type and PoP parameters if pop token is requested
            /////////////////////////////////////////////
            var tokenType = parameters.Get("token_type");
            if (tokenType != null && tokenType == "pop")
            {
                var result = ValidatePopParameters(parameters);
                if (result.IsError)
                {
                    var error = "PoP parameter validation failed: " + result.ErrorDescription;
                    LogError(error);
                    await RaiseRefreshTokenRefreshFailureEventAsync(refreshTokenHandle, error);

                    return Invalid(result.Error, result.ErrorDescription);
                }
                else
                {
                    _validatedRequest.RequestedTokenType = RequestedTokenTypes.PoP;
                }
            }

            Logger.Info("Validation of refresh token request success");
            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateCustomGrantRequestAsync(NameValueCollection parameters)
        {
            Logger.Info("Start validation of custom grant token request");

            /////////////////////////////////////////////
            // check if client is authorized for custom grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.Custom)
            {
                LogError("Client not registered for custom grant type");
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            /////////////////////////////////////////////
            // check if client is allowed grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.AllowAccessToAllCustomGrantTypes == false)
            {
                if (!_validatedRequest.Client.AllowedCustomGrantTypes.Contains(_validatedRequest.GrantType))
                {
                    LogError("Client does not have the custom grant type in the allowed list, therefore requested grant is not allowed.");
                    return Invalid(Constants.TokenErrors.UnsupportedGrantType);
                }
            }

            /////////////////////////////////////////////
            // check if a validator is registered for the grant type
            /////////////////////////////////////////////
            if (!_customGrantValidator.GetAvailableGrantTypes().Contains(_validatedRequest.GrantType, StringComparer.Ordinal))
            {
                LogError("No validator is registered for the grant type.");
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!(await ValidateRequestedScopesAsync(parameters)))
            {
                LogError("Invalid scopes.");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            /////////////////////////////////////////////
            // validate custom grant type
            /////////////////////////////////////////////
            var result = await _customGrantValidator.ValidateAsync(_validatedRequest);

            if (result == null)
            {
                LogError("Invalid custom grant.");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            if (result.IsError)
            {
                if (result.Error.IsPresent())
                {
                    LogError("Invalid custom grant: " + result.Error);
                    return Invalid(result.Error, result.ErrorDescription ?? "");
                }
                else
                {
                    LogError("Invalid custom grant.");
                    return Invalid(Constants.TokenErrors.InvalidGrant);
                }
            }
            
            if (result.Principal != null)
            {
                _validatedRequest.Subject = result.Principal;
            }

            Logger.Info("Validation of custom grant token request success");
            return Valid();
        }

        private async Task<bool> ValidateRequestedScopesAsync(NameValueCollection parameters)
        {
            var scopes = parameters.Get(Constants.TokenRequest.Scope);
            if (scopes.IsMissingOrTooLong(_options.InputLengthRestrictions.Scope))
            {
                Logger.Warn("Scopes missing or too long");
                return false;
            }

            var requestedScopes = ScopeValidator.ParseScopesString(scopes);

            if (requestedScopes == null)
            {
                return false;
            }

            if (!_scopeValidator.AreScopesAllowed(_validatedRequest.Client, requestedScopes))
            {
                return false;
            }

            if (!await _scopeValidator.AreScopesValidAsync(requestedScopes))
            {
                return false;
            }

            _validatedRequest.Scopes = requestedScopes;
            _validatedRequest.ValidatedScopes = _scopeValidator;
            return true;
        }

        private TokenRequestValidationResult ValidateAuthorizationCodeWithProofKeyParameters(string codeVerifier, AuthorizationCode authZcode)
        {
            if (authZcode.CodeChallenge.IsMissing() || authZcode.CodeChallengeMethod.IsMissing())
            {
                LogError("Client uses AuthorizationCodeWithProofKey flow but missing code challenge or code challenge method in authZ code");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            if (codeVerifier.IsMissing())
            {
                LogError("Missing code_verifier");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            if (codeVerifier.Length < _options.InputLengthRestrictions.CodeVerifierMinLength ||
                codeVerifier.Length > _options.InputLengthRestrictions.CodeVerifierMaxLength)
            {
                LogError("code_verifier is too short or too long.");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            if (Constants.SupportedCodeChallengeMethods.Contains(authZcode.CodeChallengeMethod) == false)
            {
                LogError("Unsupported code challenge method: " + authZcode.CodeChallengeMethod);
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            if (ValidateCodeVerifierAgainstCodeChallenge(codeVerifier, authZcode.CodeChallenge, authZcode.CodeChallengeMethod) == false)
            {
                LogError("Transformed code verifier does not match code challenge");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            return Valid();
        }

        private bool ValidateCodeVerifierAgainstCodeChallenge(string codeVerifier, string codeChallenge, string codeChallengeMethod)
        {
            if (codeChallengeMethod == Constants.CodeChallengeMethods.Plain)
            {
                return TimeConstantComparer.IsEqual(codeVerifier.Sha256(), codeChallenge);
            }

            var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
            var hashedBytes = codeVerifierBytes.Sha256();
            var transformedCodeVerifier = Base64Url.Encode(hashedBytes);

            return TimeConstantComparer.IsEqual(transformedCodeVerifier.Sha256(), codeChallenge);
        }

        private TokenRequestValidationResult ValidatePopParameters(NameValueCollection parameters)
        {
            var invalid = new TokenRequestValidationResult
            {
                IsError = true,
                Error = Constants.TokenErrors.InvalidRequest
            };

            // check optional alg
            var alg = parameters.Get(Constants.TokenRequest.Algorithm);
            if (alg != null)
            {
                // for now we only support asymmetric
                if (!Constants.AllowedProofKeyAlgorithms.Contains(alg))
                {
                    invalid.ErrorDescription = "invalid alg.";
                    return invalid;
                }

                _validatedRequest.ProofKeyAlgorithm = alg;
            }
            
            // key is required - for now we only support client generated keys
            var key = parameters.Get(Constants.TokenRequest.Key);
            if (key == null)
            {
                invalid.ErrorDescription = "key is required.";
                return invalid;
            }
            if (key.Length > _options.InputLengthRestrictions.ProofKey)
            {
                invalid.ErrorDescription = "invalid key.";
                Logger.Warn("Proof key exceeds max allowed length.");
                return invalid;
            }

            var jwk = string.Format("{{ \"jwk\":{0} }}", Encoding.UTF8.GetString(Base64Url.Decode(key)));
            _validatedRequest.ProofKey = jwk;

            return new TokenRequestValidationResult { IsError = false };
        }

        private TokenRequestValidationResult Valid()
        {
            return new TokenRequestValidationResult
            {
                IsError = false
            };
        }

        private TokenRequestValidationResult Invalid(string error, string errorDescription = "")
        {
            var result = new TokenRequestValidationResult
            {
                IsError = true,
                Error = error
            };

            if (errorDescription.IsPresent())
            {
                result.ErrorDescription = errorDescription;
            }

            return result;
        }

        private void LogError(string message)
        {
            Logger.Error(LogEvent(message));
        }

        private void LogWarn(string message)
        {
            Logger.Warn(LogEvent(message));
        }

        private void LogSuccess()
        {
            Logger.Info(LogEvent("Token request validation success"));
        }

        private Func<string> LogEvent(string message)
        {
            return () =>
            {
                var validationLog = new TokenRequestValidationLog(_validatedRequest);
                var json = LogSerializer.Serialize(validationLog);

                return string.Format("{0}\n {1}", message, json);
            };
        }

        private async Task RaiseSuccessfulResourceOwnerAuthenticationEventAsync(string userName, string subjectId, SignInMessage signInMessage)
        {
            await _events.RaiseSuccessfulResourceOwnerFlowAuthenticationEventAsync(userName, subjectId, signInMessage);
        }

        private async Task RaiseFailedResourceOwnerAuthenticationEventAsync(string userName, SignInMessage signInMessage, string error)
        {
            await _events.RaiseFailedResourceOwnerFlowAuthenticationEventAsync(userName, signInMessage, error);
        }

        private async Task RaiseFailedAuthorizationCodeRedeemedEventAsync(string handle, string error)
        {
            await _events.RaiseFailedAuthorizationCodeRedeemedEventAsync(_validatedRequest.Client, handle, error);
        }

        private async Task RaiseSuccessfulAuthorizationCodeRedeemedEventAsync()
        {
            await _events.RaiseSuccessAuthorizationCodeRedeemedEventAsync(_validatedRequest.Client, _validatedRequest.AuthorizationCodeHandle);
        }

        private async Task RaiseRefreshTokenRefreshFailureEventAsync(string handle, string error)
        {
            await _events.RaiseFailedRefreshTokenRefreshEventAsync(_validatedRequest.Client, handle, error);
        }
    }
}