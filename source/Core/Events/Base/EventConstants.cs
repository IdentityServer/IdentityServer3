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

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Events
{
    public static class EventConstants
    {
        public static class Categories
        {
            public const string Authentication = "Authentication";
            public const string TokenService = "TokenService";
            public const string Endpoints = "Endpoints";
            public const string Information = "Information";
            public const string InternalError = "InternalError";
        }
        
        public static class Ids
        {
            ///////////////////////////
            /// Authentication related events
            ///////////////////////////
            private const int AuthenticationEventsStart = 1000;

            public const int PreLogin = AuthenticationEventsStart + 0;
            public const int LocalLogin = AuthenticationEventsStart + 1;
            public const int ExternalLogin = AuthenticationEventsStart + 2;
            public const int Logout = AuthenticationEventsStart + 3;
            
            public const int PartialLogin = AuthenticationEventsStart + 10;
            public const int PartialLoginComplete = AuthenticationEventsStart + 11;


            ///////////////////////////
            /// Token service related events
            ///////////////////////////
            private const int TokenServiceEventsStart = 2000;

            public const int AccessTokenIssued = TokenServiceEventsStart + 0;
            public const int IdentityTokenIssued = TokenServiceEventsStart + 1;

            public const int AuthorizationCodeIssued = TokenServiceEventsStart + 10;
            public const int SuccessfulAuthorizationCodeRedeemed = TokenServiceEventsStart + 11;
            public const int FailedAuthorizationCodeRedeemed = TokenServiceEventsStart + 12;

            public const int RefreshTokenIssued = TokenServiceEventsStart + 20;
            public const int SuccessfulRefreshTokenRefreshed = TokenServiceEventsStart + 21;
            public const int FailedRefreshTokenRefreshed = TokenServiceEventsStart + 22;

            public const int PermissionRevoked = TokenServiceEventsStart + 30;
            
            
            ///////////////////////////
            /// Endpoints related events
            ///////////////////////////
            private const int EndpointsEventsStart = 3000;

            public const int SuccessfulAuthorizeEndpoint = EndpointsEventsStart + 0;
            public const int FailedAuthorizeEndpoint = EndpointsEventsStart + 1;
            
            public const int SuccessfulTokenEndpoint = EndpointsEventsStart + 2;
            public const int FailedTokenEndpoint = EndpointsEventsStart + 3;
            
            public const int SuccessfulUserInfoEndpoint = EndpointsEventsStart + 4;
            public const int FailedUserInfoEndpoint = EndpointsEventsStart + 5;

            public const int SuccessfulEndSessionEndpoint = EndpointsEventsStart + 6;
            public const int FailedEndSessionEndpoint = EndpointsEventsStart + 7;

            public const int SuccessfulIdentityTokenValidationEndpoint = EndpointsEventsStart + 8;
            public const int FailedIdentityTokenValidationEndpoint = EndpointsEventsStart + 9;

            public const int SuccessfulAccessTokenValidationEndpoint = EndpointsEventsStart + 10;
            public const int FailedAccessTokenValidationEndpoint = EndpointsEventsStart + 11;

            
            ///////////////////////////
            /// Information events
            ///////////////////////////
            private const int InformationEventsStart = 4000;

            private const int InternalError = InformationEventsStart + 0;
            private const int CertificateExpiration = InformationEventsStart + 1;

            ///////////////////////////
            /// Information events
            ///////////////////////////
            private const int InternalErrorEventsStart = 5000;

            public const int UnhandledException = InternalErrorEventsStart + 0;
        }
    }
}