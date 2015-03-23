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

using System;
using System.Collections.Generic;
using System.Net;
using Thinktecture.IdentityServer.Core.Models;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core
{
    public static class Constants
    {
        public const string IDENTITY_SERVER_NAME = "Thinktecture IdentityServer3";
        
        public const string PRIMARY_AUTHENTICATION_TYPE       = "idsrv";
        public const string EXTERNAL_AUTHENTICATION_TYPE      = "idsrv.external";
        public const string PARTIAL_SIGN_IN_AUTHENTICATION_TYPE = "idsrv.partial";
        
        internal static readonly string[] IdentityServerAuthenticationTypes = new string[]
        {
            PRIMARY_AUTHENTICATION_TYPE,
            EXTERNAL_AUTHENTICATION_TYPE,
            PARTIAL_SIGN_IN_AUTHENTICATION_TYPE
        };
        
        public const string BUILT_IN_IDENTITY_PROVIDER         = "idsrv";

        public const string ACCESS_TOKEN_AUDIENCE             = "{0}resources";

        public static readonly TimeSpan DefaultCookieTimeSpan = TimeSpan.FromHours(10);
        public static readonly TimeSpan ExternalCookieTimeSpan = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan DefaultRememberMeDuration = TimeSpan.FromDays(30);

        public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

        // the limit after which old messages are purged
        public const int SIGN_IN_MESSAGE_THRESHOLD = 5;

        public const string DEFAULT_HASH_ALGORITHM = "SHA256";

        public const string SCOPE_DISPLAY_NAME_SUFFIX = "_DisplayName";
        public const string SCOPE_DESCRIPTION_SUFFIX = "_Description";

        public const int DEFAULT_MAX_AUTHORIZE_INPUT_LENGTH = 100;
        public const int DEFAULT_MAX_TOKEN_INPUT_LENGTH = 100;

        public const int MAX_CLIENT_ID_LENGTH = DEFAULT_MAX_AUTHORIZE_INPUT_LENGTH;
        public const int MAX_SCOPE_LENGTH = 300;
        public const int MAX_REDIRECT_URI_LENGTH = 400;
        public const int MAX_NONCE_LENGTH = 300;
        public const int MAX_UI_LOCALE_LENGTH = DEFAULT_MAX_AUTHORIZE_INPUT_LENGTH;
        public const int MAX_LOGIN_HINT_LENGTH = DEFAULT_MAX_AUTHORIZE_INPUT_LENGTH;
        public const int MAX_ACR_VALUES_LENGTH = 300;
        
        public const int MAX_GRANT_TYPE_LENGTH = DEFAULT_MAX_TOKEN_INPUT_LENGTH;
        public const int MAX_USER_NAME_LENGTH = DEFAULT_MAX_TOKEN_INPUT_LENGTH;
        public const int MAX_PASSWORD_LENGTH = DEFAULT_MAX_TOKEN_INPUT_LENGTH;

        public const int MAX_CSP_REPORT_LENGTH = 2000;


        public static class AuthorizeRequest
        {
            public const string SCOPE        = "scope";
            public const string RESPONSE_TYPE = "response_type";
            public const string CLIENT_ID     = "client_id";
            public const string REDIRECT_URI  = "redirect_uri";
            public const string STATE        = "state";
            public const string RESPONSE_MODE = "response_mode";
            public const string NONCE        = "nonce";
            public const string DISPLAY      = "display";
            public const string PROMPT       = "prompt";
            public const string MAX_AGE       = "max_age";
            public const string UI_LOCALES    = "ui_locales";
            public const string ID_TOKEN_HINT  = "id_token_hint";
            public const string LOGIN_HINT    = "login_hint";
            public const string ACR_VALUES    = "acr_values";
        }

        public static class TokenRequest
        {
            public const string GRANT_TYPE    = "grant_type";
            public const string REDIRECT_URI  = "redirect_uri";
            public const string CLIENT_ID     = "client_id";
            public const string CLIENT_SECRET = "client_secret";
            public const string ASSERTION    = "assertion";
            public const string CODE         = "code";
            public const string REFRESH_TOKEN = "refresh_token";
            public const string SCOPE        = "scope";
            public const string USER_NAME     = "username";
            public const string PASSWORD     = "password";
        }

        public static class EndSessionRequest
        {
            public const string ID_TOKEN_HINT           = "id_token_hint";
            public const string POST_LOGOUT_REDIRECT_URI = "post_logout_redirect_uri";
            public const string STATE                 = "state";
        }

        public static class TokenResponse
        {
            public const string ACCESS_TOKEN   = "access_token";
            public const string IDENTITY_TOKEN = "id_token";
            public const string EXPIRES_IN     = "expires_in";
            public const string REFRESH_TOKEN  = "refresh_token";
            public const string TOKEN_TYPE     = "token_type";
            public const string STATE         = "state";
            public const string SCOPE         = "scope";
            public const string ERROR         = "error";
        }

        public static class TokenTypes
        {
            public const string ACCESS_TOKEN   = "access_token";
            public const string IDENTITY_TOKEN = "id_token";
            public const string REFRESH_TOKEN  = "refresh_token";
            public const string BEARER        = "Bearer";
        }

        public static class GrantTypes
        {
            public const string PASSWORD          = "password";
            public const string AUTHORIZATION_CODE = "authorization_code";
            public const string CLIENT_CREDENTIALS = "client_credentials";
            public const string REFRESH_TOKEN      = "refresh_token";
            public const string IMPLICIT          = "implicit";
           
            // assertion grants
            public const string SAML2_BEARER = "urn:ietf:params:oauth:grant-type:saml2-bearer";
            public const string JWT_BEARER   = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        }

        public static class ResponseTypes
        {
            // authorization code flow
            public const string CODE = "code";

            // implicit flow
            public const string TOKEN        = "token";
            public const string ID_TOKEN      = "id_token";
            public const string ID_TOKEN_TOKEN = "id_token token";
            
            // hybrid flow
            public const string CODE_ID_TOKEN      = "code id_token";
            public const string CODE_TOKEN        = "code token";
            public const string CODE_ID_TOKEN_TOKEN = "code id_token token";
        }

        public static readonly List<string> SupportedResponseTypes = new List<string> 
                            { 
                                ResponseTypes.CODE,
                                ResponseTypes.TOKEN,
                                ResponseTypes.ID_TOKEN,
                                ResponseTypes.ID_TOKEN_TOKEN,
                                ResponseTypes.CODE_ID_TOKEN,
                                ResponseTypes.CODE_TOKEN,
                                ResponseTypes.CODE_ID_TOKEN_TOKEN
                            };

        public static readonly Dictionary<string, Flows> ResponseTypeToFlowMapping = new Dictionary<string, Flows>
                            {
                                { ResponseTypes.CODE, Flows.AUTHORIZATION_CODE },
                                { ResponseTypes.TOKEN, Flows.IMPLICIT },
                                { ResponseTypes.ID_TOKEN, Flows.IMPLICIT },
                                { ResponseTypes.ID_TOKEN_TOKEN, Flows.IMPLICIT },
                                { ResponseTypes.CODE_ID_TOKEN, Flows.HYBRID },
                                { ResponseTypes.CODE_TOKEN, Flows.HYBRID },
                                { ResponseTypes.CODE_ID_TOKEN_TOKEN, Flows.HYBRID }
                            };

        public static readonly List<Flows> AllowedFlowsForAuthorizeEndpoint = new List<Flows>
                            {
                                Flows.AUTHORIZATION_CODE,
                                Flows.IMPLICIT,
                                Flows.HYBRID
                            };

        public enum ScopeRequirement
        {
            NONE, 
            RESOURCE_ONLY, 
            IDENTITY_ONLY,
            IDENTITY
        }

        public static readonly Dictionary<string, ScopeRequirement> ResponseTypeToScopeRequirement = new Dictionary<string, ScopeRequirement>
                            {
                                { ResponseTypes.CODE, ScopeRequirement.NONE },
                                { ResponseTypes.TOKEN, ScopeRequirement.RESOURCE_ONLY },
                                { ResponseTypes.ID_TOKEN, ScopeRequirement.IDENTITY_ONLY },
                                { ResponseTypes.ID_TOKEN_TOKEN, ScopeRequirement.IDENTITY },
                                { ResponseTypes.CODE_ID_TOKEN, ScopeRequirement.IDENTITY },
                                { ResponseTypes.CODE_TOKEN, ScopeRequirement.IDENTITY },
                                { ResponseTypes.CODE_ID_TOKEN_TOKEN, ScopeRequirement.IDENTITY }
                            };
                            
        public static readonly List<string> SupportedGrantTypes = new List<string> 
                            { 
                                GrantTypes.AUTHORIZATION_CODE,
                                GrantTypes.CLIENT_CREDENTIALS,
                                GrantTypes.PASSWORD,
                                GrantTypes.REFRESH_TOKEN,
                                GrantTypes.IMPLICIT
                            };

        public static readonly Dictionary<Flows, IEnumerable<string>> AllowedResponseModesForFlow = new Dictionary<Flows, IEnumerable<string>>
                            {
                                { Flows.AUTHORIZATION_CODE, new[] { ResponseModes.QUERY, ResponseModes.FORM_POST } },
                                { Flows.IMPLICIT, new[] { ResponseModes.FRAGMENT, ResponseModes.FORM_POST }},
                                { Flows.HYBRID, new[] { ResponseModes.FRAGMENT, ResponseModes.FORM_POST }}
                            };

        public static class ResponseModes
        {
            public const string FORM_POST = "form_post";
            public const string QUERY    = "query";
            public const string FRAGMENT = "fragment";
        }

        public static readonly List<string> SupportedResponseModes = new List<string>
                            {
                                ResponseModes.FORM_POST,
                                ResponseModes.QUERY,
                                ResponseModes.FRAGMENT,
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
            public const string PAGE  = "page";
            public const string POPUP = "popup";
            public const string TOUCH = "touch";
            public const string WAP   = "wap";
        }

        public static readonly List<string> SupportedDisplayModes = new List<string>
                            {
                                DisplayModes.PAGE,
                                DisplayModes.POPUP,
                                DisplayModes.TOUCH,
                                DisplayModes.WAP,
                            };

        public static class PromptModes
        {
            public const string NONE          = "none";
            public const string LOGIN         = "login";
            public const string CONSENT       = "consent";
            public const string SELECT_ACCOUNT = "select_account";
        }

        public static readonly List<string> SupportedPromptModes = new List<string>
                            {
                                PromptModes.NONE,
                                PromptModes.LOGIN,
                                PromptModes.CONSENT,
                                PromptModes.SELECT_ACCOUNT,
                            };

        public static class KnownAcrValues
        {
            public const string HOME_REALM = "idp:";
            public const string TENANT = "tenant:";
        }

        public static class AuthorizeErrors
        {
            // OAuth2 errors
            public const string INVALID_REQUEST          = "invalid_request";
            public const string UNAUTHORIZED_CLIENT      = "unauthorized_client";
            public const string ACCESS_DENIED            = "access_denied";
            public const string UNSUPPORTED_RESPONSE_TYPE = "unsupported_response_type";
            public const string INVALID_SCOPE            = "invalid_scope";
            public const string SERVER_ERROR             = "server_error";
            public const string TEMPORARILY_UNAVAILABLE  = "temporarily_unavailable";
            
            // OIDC errors
            public const string INTERACTION_REQUIRED      = "interaction_required";
            public const string LOGIN_REQUIRED            = "login_required";
            public const string ACCOUNT_SELECTION_REQUIRED = "account_selection_required";
            public const string CONSENT_REQUIRED          = "consent_required";
            public const string INVALID_REQUEST_URI        = "invalid_request_uri";
            public const string INVALID_REQUEST_OBJECT     = "invalid_request_object";
            public const string REQUEST_NOT_SUPPORTED      = "request_not_supported";
            public const string REQUEST_URI_NOT_SUPPORTED   = "request_uri_not_supported";
            public const string REGISTRATION_NOT_SUPPORTED = "registration_not_supported";
        }

        public static class TokenErrors
        {
            public const string INVALID_REQUEST          = "invalid_request";
            public const string INVALID_CLIENT           = "invalid_client";
            public const string INVALID_GRANT            = "invalid_grant";
            public const string UNAUTHORIZED_CLIENT      = "unauthorized_client";
            public const string UNSUPPORTED_GRANT_TYPE    = "unsupported_grant_type";
            public const string UNSUPPORTED_RESPONSE_TYPE = "unsupported_response_type";
            public const string INVALID_SCOPE            = "invalid_scope";
        }

        public static class ProtectedResourceErrors
        {
            public const string INVALID_TOKEN      = "invalid_token";
            public const string EXPIRED_TOKEN      = "expired_token";
            public const string INVALID_REQUEST    = "invalid_request";
            public const string INSUFFICIENT_SCOPE = "insufficient_scope";
        }

        public static Dictionary<string, HttpStatusCode> ProtectedResourceErrorStatusCodes = new Dictionary<string, HttpStatusCode>
        {
            { ProtectedResourceErrors.INVALID_TOKEN,      HttpStatusCode.Unauthorized },
            { ProtectedResourceErrors.EXPIRED_TOKEN,      HttpStatusCode.Unauthorized },
            { ProtectedResourceErrors.INVALID_REQUEST,    HttpStatusCode.BadRequest },
            { ProtectedResourceErrors.INSUFFICIENT_SCOPE, HttpStatusCode.Forbidden },
        };
        
        public static readonly Dictionary<string, IEnumerable<string>> ScopeToClaimsMapping = new Dictionary<string, IEnumerable<string>>
        {
            { StandardScopes.PROFILE, new[]
                            { 
                                ClaimTypes.NAME,
                                ClaimTypes.FAMILY_NAME,
                                ClaimTypes.GIVEN_NAME,
                                ClaimTypes.MIDDLE_NAME,
                                ClaimTypes.NICK_NAME,
                                ClaimTypes.PREFERRED_USER_NAME,
                                ClaimTypes.PROFILE,
                                ClaimTypes.PICTURE,
                                ClaimTypes.WEB_SITE,
                                ClaimTypes.GENDER,
                                ClaimTypes.BIRTH_DATE,
                                ClaimTypes.ZONE_INFO,
                                ClaimTypes.LOCALE,
                                ClaimTypes.UPDATED_AT 
                            }},
            { StandardScopes.EMAIL, new[]
                            { 
                                ClaimTypes.EMAIL,
                                ClaimTypes.EMAIL_VERIFIED 
                            }},
            { StandardScopes.ADDRESS, new[]
                            {
                                ClaimTypes.ADDRESS
                            }},
            { StandardScopes.PHONE, new[]
                            {
                                ClaimTypes.PHONE_NUMBER,
                                ClaimTypes.PHONE_NUMBER_VERIFIED
                            }},
            { StandardScopes.OPEN_ID, new[]
                            {
                                ClaimTypes.SUBJECT
                            }},
        };

        public static class StandardScopes
        {
            public const string OPEN_ID        = "openid";
            public const string PROFILE       = "profile";
            public const string EMAIL         = "email";
            public const string ADDRESS       = "address";
            public const string PHONE         = "phone";
            public const string OFFLINE_ACCESS = "offline_access";

            // not part of spec
            public const string ALL_CLAIMS     = "all_claims";
            public const string ROLES         = "roles";
        }

        public static class ClaimTypes
        {
            // core oidc claims
            public const string SUBJECT                             = "sub";
            public const string NAME                                = "name";
            public const string GIVEN_NAME                           = "given_name";
            public const string FAMILY_NAME                          = "family_name";
            public const string MIDDLE_NAME                          = "middle_name";
            public const string NICK_NAME                            = "nickname";
            public const string PREFERRED_USER_NAME                   = "preferred_username";
            public const string PROFILE                             = "profile";
            public const string PICTURE                             = "picture";
            public const string WEB_SITE                             = "website";
            public const string EMAIL                               = "email";
            public const string EMAIL_VERIFIED                       = "email_verified";
            public const string GENDER                              = "gender";
            public const string BIRTH_DATE                           = "birthdate";
            public const string ZONE_INFO                            = "zoneinfo";
            public const string LOCALE                              = "locale";
            public const string PHONE_NUMBER                         = "phone_number";
            public const string PHONE_NUMBER_VERIFIED                 = "phone_number_verified";
            public const string ADDRESS                             = "address";
            public const string AUDIENCE                            = "aud";
            public const string ISSUER                              = "iss";
            public const string NOT_BEFORE                           = "nbf";
            public const string EXPIRATION                          = "exp";
            
            // more standard claims
            public const string UPDATED_AT                           = "updated_at";
            public const string ISSUED_AT                            = "iat";
            public const string AUTHENTICATION_METHOD                = "amr";
            public const string AUTHENTICATION_CONTEXT_CLASS_REFERENCE = "acr";
            public const string AUTHENTICATION_TIME                  = "auth_time";
            public const string AUTHORIZED_PARTY                     = "azp";
            public const string ACCESS_TOKEN_HASH                     = "at_hash";
            public const string AUTHORIZATION_CODE_HASH               = "c_hash";
            public const string NONCE                               = "nonce";
            public const string JWT_ID                               = "jti";

            // more claims
            public const string CLIENT_ID         = "client_id";
            public const string SCOPE            = "scope";
            public const string ID               = "id";
            public const string SECRET           = "secret";
            public const string IDENTITY_PROVIDER = "idp";
            public const string ROLE             = "role";
            public const string REFERENCE_TOKEN_ID = "reference_token_id";

            // claims for authentication controller partial logins
            public const string AUTHORIZATION_RETURN_URL = "authorization_return_url";
            public const string PARTIAL_LOGIN_RETURN_URL = "partial_login_return_url";

            // internal claim types
            // claim type to identify external user from external provider
            public const string EXTERNAL_PROVIDER_USER_ID = "external_provider_user_id";
            public const string PARTIAL_LOGIN_RESUME_ID = "partial_login_resume_id:{0}";
        }

        public static readonly string[] ClaimsProviderFilerClaimTypes = new string[]
        {
            ClaimTypes.AUDIENCE,
            ClaimTypes.ISSUER,
            ClaimTypes.NOT_BEFORE,
            ClaimTypes.EXPIRATION,
            ClaimTypes.UPDATED_AT,
            ClaimTypes.ISSUED_AT,
            ClaimTypes.AUTHENTICATION_METHOD,
            ClaimTypes.AUTHENTICATION_TIME,
            ClaimTypes.AUTHORIZED_PARTY,
            ClaimTypes.ACCESS_TOKEN_HASH,
            ClaimTypes.AUTHORIZATION_CODE_HASH,
            ClaimTypes.NONCE,
            ClaimTypes.IDENTITY_PROVIDER
        };

        public static readonly string[] OidcProtocolClaimTypes = new string[]
        {
            ClaimTypes.SUBJECT,
            //ClaimTypes.Name,
            ClaimTypes.AUTHENTICATION_METHOD,
            ClaimTypes.IDENTITY_PROVIDER,
            ClaimTypes.AUTHENTICATION_TIME,
            ClaimTypes.AUDIENCE,
            ClaimTypes.ISSUER,
            ClaimTypes.NOT_BEFORE,
            ClaimTypes.EXPIRATION,
            ClaimTypes.UPDATED_AT,
            ClaimTypes.ISSUED_AT,
            ClaimTypes.AUTHENTICATION_CONTEXT_CLASS_REFERENCE,
            ClaimTypes.AUTHORIZED_PARTY,
            ClaimTypes.ACCESS_TOKEN_HASH,
            ClaimTypes.AUTHORIZATION_CODE_HASH,
            ClaimTypes.NONCE,
            ClaimTypes.JWT_ID,
            ClaimTypes.SCOPE,
        };

        public static readonly string[] AuthenticateResultClaimTypes = new string[]
        {
            ClaimTypes.SUBJECT,
            ClaimTypes.NAME,
            ClaimTypes.AUTHENTICATION_METHOD,
            ClaimTypes.IDENTITY_PROVIDER,
            ClaimTypes.AUTHENTICATION_TIME,
        };

        public static class AuthenticationMethods
        {
            public const string CERTIFICATE             = "certificate";
            public const string PASSWORD                = "password";
            public const string TWO_FACTOR_AUTHENTICATION = "2fa";
            public const string EXTERNAL                = "external";
        }

        public static class ClientAuthenticationMethods
        {
            public const string BASIC    = "Basic";
            public const string FORM_POST = "FormPost";
        }

        public static class RouteNames
        {
            public const string WELCOME = "idsrv.welcome";
            public const string LOGIN = "idsrv.authentication.login";
            public const string LOGIN_EXTERNAL = "idsrv.authentication.loginexternal";
            public const string LOGIN_EXTERNAL_CALLBACK = "idsrv.authentication.loginexternalcallback";
            public const string LOGOUT_PROMPT = "idsrv.authentication.logoutprompt";
            public const string LOGOUT = "idsrv.authentication.logout";
            public const string RESUME_LOGIN_FROM_REDIRECT = "idsrv.authentication.resume";
            public const string CSP_REPORT = "idsrv.csp.report";
            public const string CLIENT_PERMISSIONS = "idsrv.permissions";
            
            public static class Oidc
            {
                public const string AUTHORIZE = "idsrv.oidc.authorize";
                public const string CONSENT = "idsrv.oidc.consent";
                public const string SWITCH_USER = "idsrv.oidc.switch";
                public const string END_SESSION = "idsrv.oidc.endsession";
                public const string END_SESSION_CALLBACK = "idsrv.oidc.endsessioncallback";
                public const string CHECK_SESSION = "idsrv.oidc.checksession";
            }
        }

        public static class RoutePaths
        {
            public const string WELCOME = "";
            public const string LOGIN = "login";
            public const string LOGIN_EXTERNAL = "external";
            public const string LOGIN_EXTERNAL_CALLBACK = "callback";
            public const string LOGOUT = "logout";
            public const string RESUME_LOGIN_FROM_REDIRECT = "return";
            public const string CSP_REPORT = "csp/report";
            public const string CLIENT_PERMISSIONS = "permissions";

            public static class Oidc
            {
                public const string AUTHORIZE = "connect/authorize";
                public const string CONSENT = "connect/consent";
                public const string SWITCH_USER = "connect/switch";
                public const string DISCOVERY_CONFIGURATION = ".well-known/openid-configuration";
                public const string DISCOVERY_WEB_KEYS = ".well-known/jwks";
                public const string TOKEN = "connect/token";
                public const string REVOCATION = "connect/revocation";
                public const string USER_INFO = "connect/userinfo";
                public const string ACCESS_TOKEN_VALIDATION = "connect/accessTokenValidation";
                public const string IDENTITY_TOKEN_VALIDATION = "connect/identityTokenValidation";
                public const string END_SESSION = "connect/endsession";
                public const string END_SESSION_CALLBACK = "connect/endsessioncallback";
                public const string CHECK_SESSION = "connect/checksession";
            }
            
            public static readonly string[] CorsPaths =
            {
                Oidc.DISCOVERY_CONFIGURATION,
                Oidc.DISCOVERY_WEB_KEYS,
                Oidc.TOKEN,
                Oidc.USER_INFO,
                Oidc.IDENTITY_TOKEN_VALIDATION
            };
        }

        public static class OwinEnvironment
        {
            public const string IDENTITY_SERVER_BASE_PATH = "idsrv:IdentityServerBasePath";
            public const string IDENTITY_SERVER_HOST     = "idsrv:IdentityServerHost";

            public const string AUTOFAC_SCOPE = "idsrv:AutofacScope";
            public const string REQUEST_ID    = "idsrv:RequestId";
        }
        
        public static class Authentication
        {
            public const string SIGNIN_ID                 = "signinid";
            public const string KATANA_AUTHENTICATION_TYPE = "katanaAuthenticationType";
        }

        public static class LocalizationCategories
        {
            public const string MESSAGES = "Messages";
            public const string EVENTS   = "Events";
            public const string SCOPES   = "Scopes";
        }

        public static class TokenTypeHints
        {
            public const string REFRESH_TOKEN = "refresh_token";
            public const string ACCESS_TOKEN  = "access_token";
        }

        public static List<string> SupportedTokenTypeHints = new List<string>
        {
            TokenTypeHints.REFRESH_TOKEN,
            TokenTypeHints.ACCESS_TOKEN
        };

        public static class RevocationErrors
        {
            public const string UNSUPPORTED_TOKEN_TYPE = "unsupported_token_type";
        }
    }
}