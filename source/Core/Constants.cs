/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core
{
    public static class Constants
    {
        public const string PrimaryAuthenticationType       = "idsrv";
        public const string ExternalAuthenticationType      = "idsrv.external";
        public const string PartialSignInAuthenticationType = "idsrv.partial";
        public const string BuiltInIdentityProvider         = "idsrv";

        public const string AccessTokenAudience             = "{0}/resources";

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
            public const string Scope        = "scope";
            public const string UserName     = "username";
            public const string Password     = "password";
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
            public const string Token        = "token";
            public const string IdToken      = "id_token";
            public const string IdTokenToken = "id_token token";
            public const string Code         = "code";
        }

        public static readonly List<string> SupportedResponseTypes = new List<string> 
                            { 
                                Constants.ResponseTypes.Code,
                                Constants.ResponseTypes.Token,
                                Constants.ResponseTypes.IdToken,
                                Constants.ResponseTypes.IdTokenToken
                            };

        public static readonly List<string> SupportedGrantTypes = new List<string> 
                            { 
                                Constants.GrantTypes.AuthorizationCode,
                                Constants.GrantTypes.ClientCredentials,
                                Constants.GrantTypes.Password,
                                Constants.GrantTypes.Implicit
                            };


        public static class ResponseModes
        {
            public const string FormPost = "form_post";
            public const string Query    = "query";
            public const string Fragment = "fragment";
        }

        public static readonly List<string> SupportedResponseModes = new List<string>
                            {
                                Constants.ResponseModes.FormPost,
                                Constants.ResponseModes.Query,
                                Constants.ResponseModes.Fragment,
                            };

        public static class DisplayModes
        {
            public const string Page  = "page";
            public const string Popup = "popup";
            public const string Touch = "touch";
            public const string Wap   = "wap";
        }

        public static readonly List<string> SupportedDisplayModes = new List<string>
                            {
                                Constants.DisplayModes.Page,
                                Constants.DisplayModes.Popup,
                                Constants.DisplayModes.Touch,
                                Constants.DisplayModes.Wap,
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
                                Constants.PromptModes.None,
                                Constants.PromptModes.Login,
                                Constants.PromptModes.Consent,
                                Constants.PromptModes.SelectAccount,
                            };

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
        

        public static readonly Dictionary<string, IEnumerable<string>> ScopeToClaimsMapping = new Dictionary<string, IEnumerable<string>>
        {
            { StandardScopes.Profile, new string[]
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
            { StandardScopes.Email, new string[]
                            { 
                                ClaimTypes.Email,
                                ClaimTypes.EmailVerified 
                            }},
            { StandardScopes.Address, new string[]
                            {
                                ClaimTypes.Address
                            }},
            { StandardScopes.Phone, new string[]
                            {
                                ClaimTypes.PhoneNumber,
                                ClaimTypes.PhoneNumberVerified
                            }},
            { StandardScopes.OpenId, new string[]
                            {
                                ClaimTypes.Subject,
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

            // more claims
            public const string ClientId         = "client_id";
            public const string Scope            = "scope";
            public const string Id               = "id";
            public const string Secret           = "secret";
            public const string IdentityProvider = "idp";

            // claims for authentication controller partial logins
            public const string PartialLoginReturnUrl = "partial_login_return_url";
        }

        public static class AuthenticationMethods
        {
            public const string Certificate             = "certificate";
            public const string Password                = "password";
            public const string TwoFactorAuthentication = "2fa";
            public const string External                = "external";
        }

        public static class ClientAuthenticationMethods
        {
            public const string Basic    = "Basic";
            public const string FormPost = "FormPost";
        }

        public static class RouteNames
        {
            public const string Login                   = "idsrv.authentication.login";
            public const string LoginExternal           = "idsrv.authentication.loginexternal";
            public const string LoginExternalCallback   = "idsrv.authentication.loginexternalcallback";
            public const string LogoutPrompt            = "idsrv.authentication.logoutprompt";
            public const string Logout                  = "idsrv.authentication.logout";
            public const string ResumeLoginFromRedirect = "idsrv.authentication.resume";
        }
        
        public static class RoutePaths
        {
            public const string Login                   = "login";
            public const string LoginExternal           = "external";
            public const string LoginExternalCallback   = "callback";
            public const string Logout                  = "logout";
            public const string ResumeLoginFromRedirect = "resume";
        }
    }
}