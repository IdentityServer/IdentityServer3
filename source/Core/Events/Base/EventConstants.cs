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

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Events.Base
{
    public static class EventConstants
    {
        public static class Categories
        {
            public const string AUTHENTICATION = "Authentication";
            public const string TOKEN_SERVICE = "TokenService";
            public const string ENDPOINTS = "Endpoints";
            public const string INFORMATION = "Information";
            public const string INTERNAL_ERROR = "InternalError";
        }

        public static class EndpointNames
        {
            public const string AUTHENTICATE = "authenticate";
            public const string AUTHORIZE = "authorize";
            public const string TOKEN = "token";
            public const string REVOCATION = "revocation";
            public const string USER_INFO = "userinfo";
            public const string END_SESSION = "endsession";
            public const string ACCESS_TOKEN_VALIDATION = "accesstokenvalidation";
            public const string IDENTITY_TOKEN_VALIDATION = "identitytokenvalidaton";
            public const string CSP_REPORT = "cspreport";
            public const string CLIENT_PERMISSIONS = "clientpermissions";
        }
        
        public static class Ids
        {
            ///////////////////////////
            /// Authentication related events
            ///////////////////////////
            private const int AuthenticationEventsStart = 1000;

            public const int PRE_LOGIN_SUCCESS = AuthenticationEventsStart + 0;
            public const int PRE_LOGIN_FAILURE = AuthenticationEventsStart + 1;

            public const int LOCAL_LOGIN_SUCCESS = AuthenticationEventsStart + 10;
            public const int LOCAL_LOGIN_FAILURE = AuthenticationEventsStart + 11;

            public const int EXTERNAL_LOGIN_SUCCESS = AuthenticationEventsStart + 20;
            public const int EXTERNAL_LOGIN_FAILURE = AuthenticationEventsStart + 21;
            public const int EXTERNAL_LOGIN_ERROR = AuthenticationEventsStart + 22;
            
            public const int LOGOUT = AuthenticationEventsStart + 30;

            public const int PARTIAL_LOGIN = AuthenticationEventsStart + 40;
            public const int PARTIAL_LOGIN_COMPLETE = AuthenticationEventsStart + 41;

            public const int RESOURCE_OWNER_FLOW_LOGIN_SUCCESS = AuthenticationEventsStart + 50;
            public const int RESOURCE_OWNER_FLOW_LOGIN_FAILURE = AuthenticationEventsStart + 51;

            ///////////////////////////
            /// Token service related events
            ///////////////////////////
            private const int TokenServiceEventsStart = 2000;

            public const int ACCESS_TOKEN_ISSUED = TokenServiceEventsStart + 0;
            public const int IDENTITY_TOKEN_ISSUED = TokenServiceEventsStart + 1;

            public const int AUTHORIZATION_CODE_ISSUED = TokenServiceEventsStart + 10;
            public const int AUTHORIZATION_CODE_REDEEMED_SUCCESS = TokenServiceEventsStart + 11;
            public const int AUTHORIZATION_CODE_REDEEMED_FAILURE = TokenServiceEventsStart + 12;

            public const int REFRESH_TOKEN_ISSUED = TokenServiceEventsStart + 20;
            public const int REFRESH_TOKEN_REFRESHED_SUCCESS = TokenServiceEventsStart + 21;
            public const int REFRESH_TOKEN_REFRESHED_FAILURE = TokenServiceEventsStart + 22;

            public const int PERMISSION_REVOKED = TokenServiceEventsStart + 30;
            
            
            ///////////////////////////
            /// Endpoints related events
            ///////////////////////////
            private const int EndpointsEventsStart = 3000;

            public const int ENDPOINT_SUCCESS = EndpointsEventsStart + 0;
            public const int ENDPOINT_FAILURE = EndpointsEventsStart + 1;
            
            ///////////////////////////
            /// Information events
            ///////////////////////////
            private const int InformationEventsStart = 4000;

            public const int CERTIFICATE_EXPIRATION = InformationEventsStart + 0;
            public const int CSP_REPORT = InformationEventsStart + 1;
            public const int CLIENT_PERMISSION_REVOKED = InformationEventsStart + 2;

            public const int NO_SIGNING_CERTIFICATE_CONFIGURED = InformationEventsStart + 10;
            public const int SIGNING_CERTIFICATE_EXPIRING_SOON = InformationEventsStart + 11;
            public const int SIGNING_CERTIFICATE_VALIDATED = InformationEventsStart + 12;


            ///////////////////////////
            /// Error events
            ///////////////////////////
            private const int InternalErrorEventsStart = 5000;

            public const int UNHANDLED_EXCEPTION_ERROR = InternalErrorEventsStart + 0;
            public const int SIGNING_CERTIFICATE_PRIVAT_KEY_NOT_ACCESSIBLE = InternalErrorEventsStart + 1;
        }
    }
}