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
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Events.Authentication;
using Thinktecture.IdentityServer.Core.Events.Base;
using Thinktecture.IdentityServer.Core.Events.Informational;
using Thinktecture.IdentityServer.Core.Events.TokenService;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    internal static class IEventServiceExtensions
    {
        public static void RaisePreLoginSuccessEvent(this IEventService events, 
            string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.PreLoginSuccess,
                EventTypes.SUCCESS, 
                EventConstants.Ids.PRE_LOGIN_SUCCESS,
                new LoginDetails {
                    SubjectId = authResult.HasSubject ?  authResult.User.GetSubjectId() : null,
                    Name = authResult.User.Identity.Name,
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    PartialLogin = authResult.IsPartialSignIn
                });

            events.RaiseEvent(evt);
        }

        public static void RaisePreLoginFailureEvent(this IEventService events, 
            string signInMessageId, SignInMessage signInMessage, string error)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.PreLoginFailure,
                EventTypes.FAILURE,
                EventConstants.Ids.PRE_LOGIN_FAILURE,
                new LoginDetails
                {
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                }, 
                error);

            events.RaiseEvent(evt);
        }

        public static void RaiseLocalLoginSuccessEvent(this IEventService events, 
            string username, string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.LocalLoginSuccess,
                EventTypes.SUCCESS,
                EventConstants.Ids.LOCAL_LOGIN_SUCCESS,
                new LocalLoginDetails
                {
                    SubjectId = authResult.HasSubject ? authResult.User.GetSubjectId() : null,
                    Name = authResult.User.Identity.Name,
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    PartialLogin = authResult.IsPartialSignIn,
                    LoginUserName = username
                }); 

            events.RaiseEvent(evt);
        }

        public static void RaiseLocalLoginFailureEvent(this IEventService events, 
            string username, string signInMessageId, SignInMessage signInMessage, string error)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.LocalLoginFailure,
                EventTypes.FAILURE,
                EventConstants.Ids.LOCAL_LOGIN_FAILURE,
                new LocalLoginDetails
                {
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    LoginUserName = username
                }, 
                error); 

            events.RaiseEvent(evt);
        }

        public static void RaiseExternalLoginSuccessEvent(this IEventService events, 
            ExternalIdentity externalIdentity, string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new Event<ExternalLoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.ExternalLoginSuccess,
                EventTypes.SUCCESS,
                EventConstants.Ids.EXTERNAL_LOGIN_SUCCESS,
                new ExternalLoginDetails
                {
                    SubjectId = authResult.HasSubject ? authResult.User.GetSubjectId() : null,
                    Name = authResult.User.Identity.Name,
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    PartialLogin = authResult.IsPartialSignIn,
                    Provider = externalIdentity.Provider,
                    ProviderId = externalIdentity.ProviderId,
                }); 

            events.RaiseEvent(evt);
        }

        public static void RaiseExternalLoginFailureEvent(this IEventService events, 
            ExternalIdentity externalIdentity, string signInMessageId, SignInMessage signInMessage, string details)
        {
            var evt = new Event<ExternalLoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.ExternalLoginFailure,
                EventTypes.FAILURE,
                EventConstants.Ids.EXTERNAL_LOGIN_FAILURE,
                new ExternalLoginDetails
                {
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    Provider = externalIdentity.Provider,
                    ProviderId = externalIdentity.ProviderId,
                }); 

            events.RaiseEvent(evt);
        }

        public static void RaiseExternalLoginErrorEvent(this IEventService events, string error)
        {
            var evt = new Event<object>(
               EventConstants.Categories.AUTHENTICATION,
               Resources.Events.ExternalLoginError,
               EventTypes.ERROR,
               EventConstants.Ids.EXTERNAL_LOGIN_ERROR,
               error);

            events.RaiseEvent(evt);
        }

        public static void RaiseSuccessfulResourceOwnerFlowAuthenticationEvent(this IEventService events, 
            string userName, string subjectId, SignInMessage message)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.ResourceOwnerFlowLoginSuccess,
                EventTypes.SUCCESS,
                EventConstants.Ids.RESOURCE_OWNER_FLOW_LOGIN_SUCCESS,
                new LocalLoginDetails
                {
                    SubjectId = subjectId,
                    SignInMessage = message,
                    LoginUserName = userName
                }); 
            
            events.RaiseEvent(evt);
        }

        public static void RaiseFailedResourceOwnerFlowAuthenticationEvent(this IEventService events, 
            string userName, SignInMessage message)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.ResourceOwnerFlowLoginFailure,
                EventTypes.FAILURE,
                EventConstants.Ids.RESOURCE_OWNER_FLOW_LOGIN_FAILURE,
                new LocalLoginDetails
                {
                    SignInMessage = message,
                    LoginUserName = userName
                }); 

            events.RaiseEvent(evt);
        }

        //public static void RaisePartialLoginEvent(this IEventService events)
        //{
        //    //var evt = new ExternalLoginEvent(EventType.Failure)
        //    //{
        //    //    Provider = externalIdentity.Provider,
        //    //    ProviderId = externalIdentity.ProviderId,
        //    //    SignInMessage = signInMessage,
        //    //    Details = details
        //    //};

        //    //events.RaiseEvent(evt);
        //}

        public static void RaisePartialLoginCompleteEvent(this IEventService events, 
            ClaimsIdentity subject, string signInMessageId, SignInMessage signInMessage)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.PartialLoginComplete,
                EventTypes.INFORMATION,
                EventConstants.Ids.PARTIAL_LOGIN_COMPLETE,
                new LoginDetails
                {
                    SubjectId = subject.GetSubjectId(),
                    Name = subject.Name,
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage
                }); 

            events.RaiseEvent(evt);
        }

        public static void RaiseLogoutEvent(this IEventService events, 
            ClaimsPrincipal subject, string signOutId, SignOutMessage signOutMessage)
        {
            var evt = new Event<LogoutDetails>(
                EventConstants.Categories.AUTHENTICATION,
                Resources.Events.LogoutEvent,
                EventTypes.INFORMATION,
                EventConstants.Ids.LOGOUT,
                new LogoutDetails
                {
                    SubjectId = subject.GetSubjectId(),
                    Name = subject.Identity.Name,
                    SignOutId = signOutId,
                    SignOutMessage = signOutMessage
                });

            events.RaiseEvent(evt);
        }

        public static void RaiseCspReportEvent(this IEventService events, string report, ClaimsPrincipal user)
        {
            var evt = new Event<CspReportDetails>(
                EventConstants.Categories.INFORMATION,
                Resources.Events.CspReport,
                EventTypes.INFORMATION,
                EventConstants.Ids.CSP_REPORT);

            evt.DetailsFunc = () => {
                string subject = null;
                string name = null;
                if (user != null && user.Identity.IsAuthenticated)
                {
                    subject = user.GetSubjectId();
                    name = user.Identity.Name;
                }

                object reportData;
                try
                {
                    reportData = JsonConvert.DeserializeObject(report);
                }
                catch(JsonReaderException)
                {
                    reportData = "Error reading CSP report JSON";
                    evt.Message = "Raw Report Data: " + report;
                }
                return new CspReportDetails
                {
                    Subject = subject,
                    Name = name,
                    Report = reportData
                };
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseClientPermissionsRevokedEvent(this IEventService events, ClaimsPrincipal user, string clientId)
        {
            var evt = new Event<ClientPermissionsRevokedDetails>(
                EventConstants.Categories.INFORMATION,
                Resources.Events.ClientPermissionsRevoked,
                EventTypes.INFORMATION,
                EventConstants.Ids.CLIENT_PERMISSION_REVOKED,
                new ClientPermissionsRevokedDetails
                {
                    Subject = user.GetSubjectId(),
                    Name = user.Identity.Name,
                    ClientId = clientId
                });

            events.RaiseEvent(evt);
        }

        public static void RaiseTokenIssuedEvent(this IEventService events, Token token)
        {
            if (token.Type == Constants.TokenTypes.ACCESS_TOKEN)
            {
                events.RaiseAccessTokenIssuedEvent(token);
            }
            else if (token.Type == Constants.TokenTypes.IDENTITY_TOKEN)
            {
                events.RaiseIdentityTokenIssuedEvent(token);
            }
        }

        public static void RaiseAccessTokenIssuedEvent(this IEventService events, Token token)
        {
            var evt = new Event<AccessTokenIssuedDetails>(
                EventConstants.Categories.TOKEN_SERVICE,
                "Access token issued",
                EventTypes.INFORMATION,
                EventConstants.Ids.ACCESS_TOKEN_ISSUED) {
                    DetailsFunc = () => new AccessTokenIssuedDetails {
                        SubjectId = token.SubjectId ?? "no subject id",
                        ClientId = token.ClientId,
                        TokenType = token.Client.AccessTokenType,
                        Lifetime = token.Lifetime,
                        Scopes = token.Scopes,
                        Claims = token.Claims.ToClaimsDictionary()
                    }
                };


            events.Raise(evt);
        }

        public static void RaiseIdentityTokenIssuedEvent(this IEventService events, Token token)
        {
            var evt = new Event<TokenIssuedDetailsBase>(
                EventConstants.Categories.TOKEN_SERVICE,
                "Identity token issued",
                EventTypes.INFORMATION,
                EventConstants.Ids.IDENTITY_TOKEN_ISSUED) {
                    DetailsFunc = () => new TokenIssuedDetailsBase {
                        SubjectId = token.SubjectId,
                        ClientId = token.ClientId,
                        Lifetime = token.Lifetime,
                        Claims = token.Claims.ToClaimsDictionary()
                    }
                };


            events.RaiseEvent(evt);
        }

        public static void RaiseAuthorizationCodeIssuedEvent(this IEventService events, string id, AuthorizationCode code)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TOKEN_SERVICE,
                "Authorization code issued",
                EventTypes.INFORMATION,
                EventConstants.Ids.AUTHORIZATION_CODE_ISSUED) {
                    DetailsFunc = () => new AuthorizationCodeDetails {
                        HandleId = id,
                        ClientId = code.ClientId,
                        Scopes = code.Scopes,
                        SubjectId = code.SubjectId,
                        RedirectUri = code.RedirectUri,
                        Lifetime = code.Client.AuthorizationCodeLifetime
                    }
                };


            events.RaiseEvent(evt);
        }

        public static void RaiseRefreshTokenIssuedEvent(this IEventService events, string id, RefreshToken token)
        {
            var evt = new Event<RefreshTokenDetails>(
                EventConstants.Categories.TOKEN_SERVICE,
                "Refresh token issued",
                EventTypes.INFORMATION,
                EventConstants.Ids.REFRESH_TOKEN_ISSUED) {
                    DetailsFunc = () => new RefreshTokenDetails {
                        HandleId = id,
                        ClientId = token.ClientId,
                        Scopes = token.Scopes,
                        SubjectId = token.SubjectId,
                        Lifetime = token.LifeTime,
                        Version = token.Version
                    }
                };


            events.RaiseEvent(evt);
        }

        public static void RaiseSuccessfulRefreshTokenRefreshEvent(this IEventService events, string oldHandle, string newHandle, RefreshToken token)
        {
            var evt = new Event<RefreshTokenRefreshDetails>(
                EventConstants.Categories.TOKEN_SERVICE,
                "Refresh token refresh success",
                EventTypes.SUCCESS,
                EventConstants.Ids.REFRESH_TOKEN_REFRESHED_SUCCESS) {
                    Details = new RefreshTokenRefreshDetails {
                        OldHandle = oldHandle,
                        NewHandle = newHandle,
                        ClientId = token.ClientId,
                        Lifetime = token.LifeTime
                    }
                };


            events.RaiseEvent(evt);
        }

        public static void RaiseUnhandledExceptionEvent(this IEventService events, Exception exception)
        {
            var evt = new Event<object>(
                EventConstants.Categories.INTERNAL_ERROR,
                "Unhandled exception",
                EventTypes.ERROR,
                EventConstants.Ids.UNHANDLED_EXCEPTION_ERROR, 
                exception.ToString());

            events.RaiseEvent(evt);
        }

        public static void RaiseSuccessfulEndpointEvent(this IEventService events, string endpointName)
        {
            var evt = new Event<EndpointDetail>(
                EventConstants.Categories.ENDPOINTS,
                "Endpoint success",
                EventTypes.SUCCESS,
                EventConstants.Ids.ENDPOINT_SUCCESS,
                new EndpointDetail { EndpointName = endpointName });

            events.Raise(evt);
        }

        public static void RaiseFailureEndpointEvent(this IEventService events, string endpointName, string error)
        {
            var evt = new Event<EndpointDetail>(
                 EventConstants.Categories.ENDPOINTS,
                 "Endpoint failure",
                 EventTypes.FAILURE,
                 EventConstants.Ids.ENDPOINT_FAILURE,
                 new EndpointDetail { EndpointName = endpointName },
                 error);

            events.Raise(evt);
        }

        public static void RaiseFailedAuthorizationCodeRedeemedEvent(this IEventService events, Client client, string handle, string error)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TOKEN_SERVICE,
                "Authorization code redeem failure",
                EventTypes.FAILURE,
                EventConstants.Ids.AUTHORIZATION_CODE_REDEEMED_FAILURE,
                new AuthorizationCodeDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                },
                error);

            events.Raise(evt);
        }

        public static void RaiseSuccessAuthorizationCodeRedeemedEvent(this IEventService events, Client client, string handle)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TOKEN_SERVICE,
                "Authorization code redeem success",
                EventTypes.SUCCESS,
                EventConstants.Ids.AUTHORIZATION_CODE_REDEEMED_SUCCESS,
                new AuthorizationCodeDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                });

            events.Raise(evt);
        }

        public static void RaiseFailedRefreshTokenRefreshEvent(this IEventService events, Client client, string handle, string error)
        {
            var evt = new Event<RefreshTokenDetails>(
                EventConstants.Categories.TOKEN_SERVICE,
                "Refresh token refresh failure",
                EventTypes.FAILURE,
                EventConstants.Ids.REFRESH_TOKEN_REFRESHED_FAILURE,
                new RefreshTokenDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                },
                error);

            events.Raise(evt);
        }

        public static void RaiseSuccessRefreshTokenRefreshEvent(this IEventService events, Client client, string handle)
        {
            
        }

        public static void RaiseNoCertificateConfiguredEvent(this IEventService events)
        {
            var evt = new Event<object>(
                EventConstants.Categories.INFORMATION,
                "No signing certificate configured",
                EventTypes.INFORMATION,
                EventConstants.Ids.NO_SIGNING_CERTIFICATE_CONFIGURED);

            events.Raise(evt);
        }

        public static void RaiseCertificatePrivateKeyNotAccessibleEvent(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.INTERNAL_ERROR,
                "Signing certificate has no private key, or key is not accessible",
                EventTypes.ERROR,
                EventConstants.Ids.SIGNING_CERTIFICATE_PRIVAT_KEY_NOT_ACCESSIBLE,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                },
                "Make sure the account running your application has access to the private key");

            events.Raise(evt);
        }

        public static void RaiseCertificateKeyLengthTooShortEvent(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.INTERNAL_ERROR,
                "Signing certificate key length is less than 2048 bits.",
                EventTypes.ERROR,
                EventConstants.Ids.SIGNING_CERTIFICATE_PRIVAT_KEY_NOT_ACCESSIBLE,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            events.Raise(evt);
        }

        public static void RaiseCertificateExpiringSoonEvent(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.INFORMATION,
                "The signing certificate will expire in the next 30 days",
                EventTypes.INFORMATION,
                EventConstants.Ids.SIGNING_CERTIFICATE_EXPIRING_SOON,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            events.Raise(evt);
        }

        public static void RaiseCertificateValidatedEvent(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.INFORMATION,
                "Signing certificate validation success",
                EventTypes.INFORMATION,
                EventConstants.Ids.SIGNING_CERTIFICATE_VALIDATED,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            events.Raise(evt);
        }

        private static void RaiseEvent<T>(this IEventService events, Event<T> evt)
        {
            if (events == null) throw new ArgumentNullException("events");

            events.Raise(evt);
        }
    }
}