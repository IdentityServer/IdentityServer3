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
using System;
using System.Collections.Generic;
using System.Net;

#pragma warning disable 1591

namespace IdentityServer3.Core
{
    public static class Constants
    {
        public const string IdentityServerName = "IdentityServer3";
        
        public const string PrimaryAuthenticationType       = "idsrv";
        public const string ExternalAuthenticationType      = "idsrv.external";
        public const string PartialSignInAuthenticationType = "idsrv.partial";
        
        internal static readonly string[] IdentityServerAuthenticationTypes = new string[]
        {
            PrimaryAuthenticationType,
            ExternalAuthenticationType,
            PartialSignInAuthenticationType
        };
        
        public const string BuiltInIdentityProvider         = "idsrv";

        public const string AccessTokenAudience             = "{0}resources";

        public static readonly TimeSpan DefaultCookieTimeSpan = TimeSpan.FromHours(10);
        public static readonly TimeSpan ExternalCookieTimeSpan = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan DefaultRememberMeDuration = TimeSpan.FromDays(30);

        public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

        // the limit after which old messages are purged
        public const int SignInMessageThreshold = 5;

        public const string DefaultHashAlgorithm = "SHA256";

        public const string ScopeDisplayNameSuffix = "_DisplayName";
        public const string ScopeDescriptionSuffix = "_Description";

        public const int DefaultMaxAuthorizeInputLength = 100;
        public const int DefaultMaxTokenInputLength = 100;

        public const int MaxClientIdLength = DefaultMaxAuthorizeInputLength;
        public const int MaxScopeLength = 300;
        public const int MaxRedirectUriLength = 400;
        public const int MaxNonceLength = 300;
        public const int MaxUiLocaleLength = DefaultMaxAuthorizeInputLength;
        public const int MaxLoginHintLength = DefaultMaxAuthorizeInputLength;
        public const int MaxAcrValuesLength = 300;
        
        public const int MaxGrantTypeLength = DefaultMaxTokenInputLength;
        public const int MaxUserNameLength = DefaultMaxTokenInputLength;
        public const int MaxPasswordLength = DefaultMaxTokenInputLength;

        public const int MaxCspReportLength = 2000;


        public static class AuthorizeRequest
        {
            public const string Scope        = "scope";
            public const string ResponseType = "response_type";
            public const string ClientId     = "client_id";
            public const string RedirectUri  = "redirect_uri";
            public const string State        = "state";
            public const string ResponseMode = "response_mode";
            public const string Nonce        = "nonce";
            public const string Display      = "display";
            public const string Prompt       = "prompt";
            public const string MaxAge       = "max_age";
            public const string UiLocales    = "ui_locales";
            public const string IdTokenHint  = "id_token_hint";
            public const string LoginHint    = "login_hint";
            public const string AcrValues    = "acr_values";
        }

        public static class TokenRequest
        {
            public const string GrantType    = "grant_type";
            public const string RedirectUri  = "redirect_uri";
            public const string ClientId     = "client_id";
            public const string ClientSecret = "client_secret";
            public const string Assertion    = "assertion";
            public const string Code         = "code";
            public const string RefreshToken = "refresh_token";
            public const string Scope        = "scope";
            public const string UserName     = "username";
            public const string Password     = "password";
        }

        public static class EndSessionRequest
        {
            public const string IdTokenHint           = "id_token_hint";
            public const string PostLogoutRedirectUri = "post_logout_redirect_uri";
            public const string State                 = "state";
        }

        public static class TokenResponse
        {
            public const string AccessToken   = "access_token";
            public const string IdentityToken = "id_token";
            public const string ExpiresIn     = "expires_in";
            public const string RefreshToken  = "refresh_token";
            public const string TokenType     = "token_type";
            public const string State         = "state";
            public const string Scope         = "scope";
            public const string Error         = "error";
        }

        public static class TokenTypes
        {
            public const string AccessToken   = "access_token";
            public const string IdentityToken = "id_token";
            public const string RefreshToken  = "refresh_token";
            public const string Bearer        = "Bearer";
        }

        public static class GrantTypes
        {
            public const string Password          = "password";
            public const string AuthorizationCode = "authorization_code";
            public const string ClientCredentials = "client_credentials";
            public const string RefreshToken      = "refresh_token";
            public const string Implicit          = "implicit";
           
            // assertion grants
            public const string Saml2Bearer = "urn:ietf:params:oauth:grant-type:saml2-bearer";
            public const string JwtBearer   = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        }

        public static class ResponseTypes
        {
            // authorization code flow
            public const string Code = "code";

            // implicit flow
            public const string Token        = "token";
            public const string IdToken      = "id_token";
            public const string IdTokenToken = "id_token token";
            
            // hybrid flow
            public const string CodeIdToken      = "code id_token";
            public const string CodeToken        = "code token";
            public const string CodeIdTokenToken = "code id_token token";
        }

        public static readonly List<string> SupportedResponseTypes = new List<string> 
                            { 
                                ResponseTypes.Code,
                                ResponseTypes.Token,
                                ResponseTypes.IdToken,
                                ResponseTypes.IdTokenToken,
                                ResponseTypes.CodeIdToken,
                                ResponseTypes.CodeToken,
                                ResponseTypes.CodeIdTokenToken
                            };

        public static readonly Dictionary<string, Flows> ResponseTypeToFlowMapping = new Dictionary<string, Flows>
                            {
                                { ResponseTypes.Code, Flows.AuthorizationCode },
                                { ResponseTypes.Token, Flows.Implicit },
                                { ResponseTypes.IdToken, Flows.Implicit },
                                { ResponseTypes.IdTokenToken, Flows.Implicit },
                                { ResponseTypes.CodeIdToken, Flows.Hybrid },
                                { ResponseTypes.CodeToken, Flows.Hybrid },
                                { ResponseTypes.CodeIdTokenToken, Flows.Hybrid }
                            };

        public static readonly List<Flows> AllowedFlowsForAuthorizeEndpoint = new List<Flows>
                            {
                                Flows.AuthorizationCode,
                                Flows.Implicit,
                                Flows.Hybrid
                            };

        public enum ScopeRequirement
        {
            None, 
            ResourceOnly, 
            IdentityOnly,
            Identity
        }

        public static readonly Dictionary<string, ScopeRequirement> ResponseTypeToScopeRequirement = new Dictionary<string, ScopeRequirement>
                            {
                                { ResponseTypes.Code, ScopeRequirement.None },
                                { ResponseTypes.Token, ScopeRequirement.ResourceOnly },
                                { ResponseTypes.IdToken, ScopeRequirement.IdentityOnly },
                                { ResponseTypes.IdTokenToken, ScopeRequirement.Identity },
                                { ResponseTypes.CodeIdToken, ScopeRequirement.Identity },
                                { ResponseTypes.CodeToken, ScopeRequirement.Identity },
                                { ResponseTypes.CodeIdTokenToken, ScopeRequirement.Identity }
                            };
                            
        public static readonly List<string> SupportedGrantTypes = new List<string> 
                            { 
                                GrantTypes.AuthorizationCode,
                                GrantTypes.ClientCredentials,
                                GrantTypes.Password,
                                GrantTypes.RefreshToken,
                                GrantTypes.Implicit
                            };

        public static readonly Dictionary<Flows, IEnumerable<string>> AllowedResponseModesForFlow = new Dictionary<Flows, IEnumerable<string>>
                            {
                                { Flows.AuthorizationCode, new[] { ResponseModes.Query, ResponseModes.FormPost } },
                                { Flows.Implicit, new[] { ResponseModes.Fragment, ResponseModes.FormPost }},
                                { Flows.Hybrid, new[] { ResponseModes.Fragment, ResponseModes.FormPost }}
                            };

        public static class ResponseModes
        {
            public const string FormPost = "form_post";
            public const string Query    = "query";
            public const string Fragment = "fragment";
        }

        public static readonly List<string> SupportedResponseModes = new List<string>
                            {
                                ResponseModes.FormPost,
                                ResponseModes.Query,
                                ResponseModes.Fragment,
                            };

        public static string[] SupportedSubjectTypes =
        {
            "pairwise", "public"
        };

        public static class SigningAlgorithms
        {
            public const string RSA_SHA_256 = "RS256";
        }

        public static class DisplayModes
        {
            public const string Page  = "page";
            public const string Popup = "popup";
            public const string Touch = "touch";
            public const string Wap   = "wap";
        }

        public static readonly List<string> SupportedDisplayModes = new List<string>
                            {
                                DisplayModes.Page,
                                DisplayModes.Popup,
                                DisplayModes.Touch,
                                DisplayModes.Wap,
                            };

        public static class PromptModes
        {
            public const string None          = "none";
            public const string Login         = "login";
            public const string Consent       = "consent";
            public const string SelectAccount = "select_account";
        }

        public static readonly List<string> SupportedPromptModes = new List<string>
                            {
                                PromptModes.None,
                                PromptModes.Login,
                                PromptModes.Consent,
                                PromptModes.SelectAccount,
                            };

        public static class KnownAcrValues
        {
            public const string HomeRealm = "idp:";
            public const string Tenant = "tenant:";
        }

        public static class AuthorizeErrors
        {
            // OAuth2 errors
            public const string InvalidRequest          = "invalid_request";
            public const string UnauthorizedClient      = "unauthorized_client";
            public const string AccessDenied            = "access_denied";
            public const string UnsupportedResponseType = "unsupported_response_type";
            public const string InvalidScope            = "invalid_scope";
            public const string ServerError             = "server_error";
            public const string TemporarilyUnavailable  = "temporarily_unavailable";
            
            // OIDC errors
            public const string InteractionRequired      = "interaction_required";
            public const string LoginRequired            = "login_required";
            public const string AccountSelectionRequired = "account_selection_required";
            public const string ConsentRequired          = "consent_required";
            public const string InvalidRequestUri        = "invalid_request_uri";
            public const string InvalidRequestObject     = "invalid_request_object";
            public const string RequestNotSupported      = "request_not_supported";
            public const string RequestUriNotSupported   = "request_uri_not_supported";
            public const string RegistrationNotSupported = "registration_not_supported";
        }

        public static class TokenErrors
        {
            public const string InvalidRequest          = "invalid_request";
            public const string InvalidClient           = "invalid_client";
            public const string InvalidGrant            = "invalid_grant";
            public const string UnauthorizedClient      = "unauthorized_client";
            public const string UnsupportedGrantType    = "unsupported_grant_type";
            public const string UnsupportedResponseType = "unsupported_response_type";
            public const string InvalidScope            = "invalid_scope";
        }

        public static class ProtectedResourceErrors
        {
            public const string InvalidToken      = "invalid_token";
            public const string ExpiredToken      = "expired_token";
            public const string InvalidRequest    = "invalid_request";
            public const string InsufficientScope = "insufficient_scope";
        }

        public static Dictionary<string, HttpStatusCode> ProtectedResourceErrorStatusCodes = new Dictionary<string, HttpStatusCode>
        {
            { ProtectedResourceErrors.InvalidToken,      HttpStatusCode.Unauthorized },
            { ProtectedResourceErrors.ExpiredToken,      HttpStatusCode.Unauthorized },
            { ProtectedResourceErrors.InvalidRequest,    HttpStatusCode.BadRequest },
            { ProtectedResourceErrors.InsufficientScope, HttpStatusCode.Forbidden },
        };
        
        public static readonly Dictionary<string, IEnumerable<string>> ScopeToClaimsMapping = new Dictionary<string, IEnumerable<string>>
        {
            { StandardScopes.Profile, new[]
                            { 
                                ClaimTypes.Name,
                                ClaimTypes.FamilyName,
                                ClaimTypes.GivenName,
                                ClaimTypes.MiddleName,
                                ClaimTypes.NickName,
                                ClaimTypes.PreferredUserName,
                                ClaimTypes.Profile,
                                ClaimTypes.Picture,
                                ClaimTypes.WebSite,
                                ClaimTypes.Gender,
                                ClaimTypes.BirthDate,
                                ClaimTypes.ZoneInfo,
                                ClaimTypes.Locale,
                                ClaimTypes.UpdatedAt 
                            }},
            { StandardScopes.Email, new[]
                            { 
                                ClaimTypes.Email,
                                ClaimTypes.EmailVerified 
                            }},
            { StandardScopes.Address, new[]
                            {
                                ClaimTypes.Address
                            }},
            { StandardScopes.Phone, new[]
                            {
                                ClaimTypes.PhoneNumber,
                                ClaimTypes.PhoneNumberVerified
                            }},
            { StandardScopes.OpenId, new[]
                            {
                                ClaimTypes.Subject
                            }},
        };

        public static class StandardScopes
        {
            public const string OpenId        = "openid";
            public const string Profile       = "profile";
            public const string Email         = "email";
            public const string Address       = "address";
            public const string Phone         = "phone";
            public const string OfflineAccess = "offline_access";

            // not part of spec
            public const string AllClaims     = "all_claims";
            public const string Roles         = "roles";
        }

        public static class ClaimTypes
        {
            // core oidc claims
            public const string Subject                             = "sub";
            public const string Name                                = "name";
            public const string GivenName                           = "given_name";
            public const string FamilyName                          = "family_name";
            public const string MiddleName                          = "middle_name";
            public const string NickName                            = "nickname";
            public const string PreferredUserName                   = "preferred_username";
            public const string Profile                             = "profile";
            public const string Picture                             = "picture";
            public const string WebSite                             = "website";
            public const string Email                               = "email";
            public const string EmailVerified                       = "email_verified";
            public const string Gender                              = "gender";
            public const string BirthDate                           = "birthdate";
            public const string ZoneInfo                            = "zoneinfo";
            public const string Locale                              = "locale";
            public const string PhoneNumber                         = "phone_number";
            public const string PhoneNumberVerified                 = "phone_number_verified";
            public const string Address                             = "address";
            public const string Audience                            = "aud";
            public const string Issuer                              = "iss";
            public const string NotBefore                           = "nbf";
            public const string Expiration                          = "exp";
            
            // more standard claims
            public const string UpdatedAt                           = "updated_at";
            public const string IssuedAt                            = "iat";
            public const string AuthenticationMethod                = "amr";
            public const string AuthenticationContextClassReference = "acr";
            public const string AuthenticationTime                  = "auth_time";
            public const string AuthorizedParty                     = "azp";
            public const string AccessTokenHash                     = "at_hash";
            public const string AuthorizationCodeHash               = "c_hash";
            public const string Nonce                               = "nonce";
            public const string JwtId                               = "jti";

            // more claims
            public const string ClientId         = "client_id";
            public const string Scope            = "scope";
            public const string Id               = "id";
            public const string Secret           = "secret";
            public const string IdentityProvider = "idp";
            public const string Role             = "role";
            public const string ReferenceTokenId = "reference_token_id";

            // claims for authentication controller partial logins
            public const string AuthorizationReturnUrl = "authorization_return_url";
            public const string PartialLoginRestartUrl = "partial_login_restart_url";
            public const string PartialLoginReturnUrl = "partial_login_return_url";

            // internal claim types
            // claim type to identify external user from external provider
            public const string ExternalProviderUserId = "external_provider_user_id";
            public const string PartialLoginResumeId = PartialLoginResumeClaimPrefix + "{0}";
        }

        public const string PartialLoginResumeClaimPrefix = "partial_login_resume_id:";

        public static readonly string[] ClaimsProviderFilerClaimTypes = new string[]
        {
            ClaimTypes.Audience,
            ClaimTypes.Issuer,
            ClaimTypes.NotBefore,
            ClaimTypes.Expiration,
            ClaimTypes.UpdatedAt,
            ClaimTypes.IssuedAt,
            ClaimTypes.AuthenticationMethod,
            ClaimTypes.AuthenticationTime,
            ClaimTypes.AuthorizedParty,
            ClaimTypes.AccessTokenHash,
            ClaimTypes.AuthorizationCodeHash,
            ClaimTypes.Nonce,
            ClaimTypes.IdentityProvider
        };

        public static readonly string[] OidcProtocolClaimTypes = new string[]
        {
            ClaimTypes.Subject,
            //ClaimTypes.Name,
            ClaimTypes.AuthenticationMethod,
            ClaimTypes.IdentityProvider,
            ClaimTypes.AuthenticationTime,
            ClaimTypes.Audience,
            ClaimTypes.Issuer,
            ClaimTypes.NotBefore,
            ClaimTypes.Expiration,
            ClaimTypes.UpdatedAt,
            ClaimTypes.IssuedAt,
            ClaimTypes.AuthenticationContextClassReference,
            ClaimTypes.AuthorizedParty,
            ClaimTypes.AccessTokenHash,
            ClaimTypes.AuthorizationCodeHash,
            ClaimTypes.Nonce,
            ClaimTypes.JwtId,
            ClaimTypes.Scope,
        };

        public static readonly string[] AuthenticateResultClaimTypes = new string[]
        {
            ClaimTypes.Subject,
            ClaimTypes.Name,
            ClaimTypes.AuthenticationMethod,
            ClaimTypes.IdentityProvider,
            ClaimTypes.AuthenticationTime,
        };

        public static class AuthenticationMethods
        {
            public const string Certificate             = "certificate";
            public const string Password                = "password";
            public const string TwoFactorAuthentication = "2fa";
            public const string External                = "external";
        }

        public static class ParsedSecretTypes
        {
            public const string SharedSecret = "SharedSecret";
            public const string X509Certificate = "X509Certificate";
        }

        public static class SecretTypes
        {
            public const string SharedSecret              = "SharedSecret";
            public const string X509CertificateThumbprint = "X509Thumbprint";
            public const string X509CertificateName       = "X509Name";
        }

        public static class TokenEndpointAuthenticationMethods
        {
            public const string PostBody = "client_secret_post";
            public const string BasicAuthentication = "client_secret_basic";
        }

        public static class RouteNames
        {
            public const string Welcome = "idsrv.welcome";
            public const string Login = "idsrv.authentication.login";
            public const string LoginExternal = "idsrv.authentication.loginexternal";
            public const string LoginExternalCallback = "idsrv.authentication.loginexternalcallback";
            public const string LogoutPrompt = "idsrv.authentication.logoutprompt";
            public const string Logout = "idsrv.authentication.logout";
            public const string ResumeLoginFromRedirect = "idsrv.authentication.resume";
            public const string CspReport = "idsrv.csp.report";
            public const string ClientPermissions = "idsrv.permissions";
            
            public static class Oidc
            {
                public const string Authorize = "idsrv.oidc.authorize";
                public const string Consent = "idsrv.oidc.consent";
                public const string SwitchUser = "idsrv.oidc.switch";
                public const string EndSession = "idsrv.oidc.endsession";
                public const string EndSessionCallback = "idsrv.oidc.endsessioncallback";
                public const string CheckSession = "idsrv.oidc.checksession";
            }
        }

        public static class RoutePaths
        {
            public const string Welcome = "";
            public const string Login = "login";
            public const string LoginExternal = "external";
            public const string LoginExternalCallback = "callback";
            public const string Logout = "logout";
            public const string ResumeLoginFromRedirect = "return";
            public const string CspReport = "csp/report";
            public const string ClientPermissions = "permissions";

            public static class Oidc
            {
                public const string Authorize = "connect/authorize";
                public const string Consent = "connect/consent";
                public const string SwitchUser = "connect/switch";
                public const string DiscoveryConfiguration = ".well-known/openid-configuration";
                public const string DiscoveryWebKeys = ".well-known/jwks";
                public const string Token = "connect/token";
                public const string Revocation = "connect/revocation";
                public const string UserInfo = "connect/userinfo";
                public const string AccessTokenValidation = "connect/accessTokenValidation";
                public const string IdentityTokenValidation = "connect/identityTokenValidation";
                public const string EndSession = "connect/endsession";
                public const string EndSessionCallback = "connect/endsessioncallback";
                public const string CheckSession = "connect/checksession";
            }
            
            public static readonly string[] CorsPaths =
            {
                Oidc.DiscoveryConfiguration,
                Oidc.DiscoveryWebKeys,
                Oidc.Token,
                Oidc.UserInfo,
                Oidc.IdentityTokenValidation
            };
        }

        public static class OwinEnvironment
        {
            public const string IdentityServerBasePath = "idsrv:IdentityServerBasePath";
            public const string IdentityServerHost     = "idsrv:IdentityServerHost";

            public const string RequestId    = "idsrv:RequestId";
        }
        
        public static class Authentication
        {
            public const string SigninId                 = "signinid";
            public const string KatanaAuthenticationType = "katanaAuthenticationType";
        }

        public static class LocalizationCategories
        {
            public const string Messages = "Messages";
            public const string Events   = "Events";
            public const string Scopes   = "Scopes";
        }

        public static class TokenTypeHints
        {
            public const string RefreshToken = "refresh_token";
            public const string AccessToken  = "access_token";
        }

        public static List<string> SupportedTokenTypeHints = new List<string>
        {
            TokenTypeHints.RefreshToken,
            TokenTypeHints.AccessToken
        };

        public static class RevocationErrors
        {
            public const string UnsupportedTokenType = "unsupported_token_type";
        }

        public static class ProfileDataCallers
        {
            public const string UserInfoEndpoint = "UserInfoEndpoint";
            public const string ClaimsProviderIdentityToken = "ClaimsProviderIdentityToken";
            public const string ClaimsProviderAccessToken = "ClaimsProviderAccessToken";
        }
    }
}