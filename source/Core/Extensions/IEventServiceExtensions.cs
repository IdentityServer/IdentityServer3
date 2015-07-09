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

using IdentityServer3.Core.Events;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Extensions
{
    internal static class IEventServiceExtensions
    {
        public static async Task RaisePreLoginSuccessEventAsync(this IEventService events, 
            string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.PreLoginSuccess,
                EventTypes.Success, 
                EventConstants.Ids.PreLoginSuccess,
                new LoginDetails {
                    SubjectId = authResult.HasSubject ?  authResult.User.GetSubjectId() : null,
                    Name = authResult.User.Identity.Name,
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    PartialLogin = authResult.IsPartialSignIn
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaisePreLoginFailureEventAsync(this IEventService events, 
            string signInMessageId, SignInMessage signInMessage, string error)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.PreLoginFailure,
                EventTypes.Failure,
                EventConstants.Ids.PreLoginFailure,
                new LoginDetails
                {
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                }, 
                error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseLocalLoginSuccessEventAsync(this IEventService events, 
            string username, string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.LocalLoginSuccess,
                EventTypes.Success,
                EventConstants.Ids.LocalLoginSuccess,
                new LocalLoginDetails
                {
                    SubjectId = authResult.HasSubject ? authResult.User.GetSubjectId() : null,
                    Name = authResult.User.Identity.Name,
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    PartialLogin = authResult.IsPartialSignIn,
                    LoginUserName = username
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseLocalLoginFailureEventAsync(this IEventService events, 
            string username, string signInMessageId, SignInMessage signInMessage, string error)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.LocalLoginFailure,
                EventTypes.Failure,
                EventConstants.Ids.LocalLoginFailure,
                new LocalLoginDetails
                {
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    LoginUserName = username
                }, 
                error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseExternalLoginSuccessEventAsync(this IEventService events, 
            ExternalIdentity externalIdentity, string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new Event<ExternalLoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.ExternalLoginSuccess,
                EventTypes.Success,
                EventConstants.Ids.ExternalLoginSuccess,
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

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseExternalLoginFailureEventAsync(this IEventService events, 
            ExternalIdentity externalIdentity, string signInMessageId, SignInMessage signInMessage, string error)
        {
            var evt = new Event<ExternalLoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.ExternalLoginFailure,
                EventTypes.Failure,
                EventConstants.Ids.ExternalLoginFailure,
                new ExternalLoginDetails
                {
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage,
                    Provider = externalIdentity.Provider,
                    ProviderId = externalIdentity.ProviderId,
                }, 
                error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseExternalLoginErrorEventAsync(this IEventService events, string error)
        {
            var evt = new Event<object>(
               EventConstants.Categories.Authentication,
               Resources.Events.ExternalLoginError,
               EventTypes.Error,
               EventConstants.Ids.ExternalLoginError,
               error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseSuccessfulResourceOwnerFlowAuthenticationEventAsync(this IEventService events, 
            string userName, string subjectId, SignInMessage message)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.ResourceOwnerFlowLoginSuccess,
                EventTypes.Success,
                EventConstants.Ids.ResourceOwnerFlowLoginSuccess,
                new LocalLoginDetails
                {
                    SubjectId = subjectId,
                    SignInMessage = message,
                    LoginUserName = userName
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseFailedResourceOwnerFlowAuthenticationEventAsync(this IEventService events, 
            string userName, SignInMessage message, string error)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.ResourceOwnerFlowLoginFailure,
                EventTypes.Failure,
                EventConstants.Ids.ResourceOwnerFlowLoginFailure,
                new LocalLoginDetails
                {
                    SignInMessage = message,
                    LoginUserName = userName
                },
                error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaisePartialLoginCompleteEventAsync(this IEventService events, 
            ClaimsIdentity subject, string signInMessageId, SignInMessage signInMessage)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.PartialLoginComplete,
                EventTypes.Information,
                EventConstants.Ids.PartialLoginComplete,
                new LoginDetails
                {
                    SubjectId = subject.GetSubjectId(),
                    Name = subject.Name,
                    SignInId = signInMessageId,
                    SignInMessage = signInMessage
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseLogoutEventAsync(this IEventService events, 
            ClaimsPrincipal subject, string signOutId, SignOutMessage signOutMessage)
        {
            var evt = new Event<LogoutDetails>(
                EventConstants.Categories.Authentication,
                Resources.Events.LogoutEvent,
                EventTypes.Information,
                EventConstants.Ids.Logout,
                new LogoutDetails
                {
                    SubjectId = subject.GetSubjectId(),
                    Name = subject.Identity.Name,
                    SignOutId = signOutId,
                    SignOutMessage = signOutMessage
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseCspReportEventAsync(this IEventService events, string report, ClaimsPrincipal user)
        {
            var evt = new Event<CspReportDetails>(
                EventConstants.Categories.Information,
                Resources.Events.CspReport,
                EventTypes.Information,
                EventConstants.Ids.CspReport);

            evt.DetailsFunc = () => {
                string subject = null;
                string name = null;
                if (user != null && user.Identity.IsAuthenticated)
                {
                    subject = user.GetSubjectId();
                    name = user.Identity.Name;
                }

                object reportData = null;
                try
                {
                    reportData = Newtonsoft.Json.JsonConvert.DeserializeObject(report);
                }
                catch(Newtonsoft.Json.JsonReaderException)
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

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseClientPermissionsRevokedEventAsync(this IEventService events, ClaimsPrincipal user, string clientId)
        {
            var evt = new Event<ClientPermissionsRevokedDetails>(
                EventConstants.Categories.Information,
                Resources.Events.ClientPermissionsRevoked,
                EventTypes.Information,
                EventConstants.Ids.ClientPermissionRevoked,
                new ClientPermissionsRevokedDetails
                {
                    Subject = user.GetSubjectId(),
                    Name = user.Identity.Name,
                    ClientId = clientId
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseTokenIssuedEventAsync(this IEventService events, Token token)
        {
            if (token.Type == Constants.TokenTypes.AccessToken)
            {
                await events.RaiseAccessTokenIssuedEventAsync(token);
            }
            else if (token.Type == Constants.TokenTypes.IdentityToken)
            {
                await events.RaiseIdentityTokenIssuedEventAsync(token);
            }
        }

        public static async Task RaiseAccessTokenIssuedEventAsync(this IEventService events, Token token)
        {
            var evt = new Event<AccessTokenIssuedDetails>(
                EventConstants.Categories.TokenService,
                "Access token issued",
                EventTypes.Information,
                EventConstants.Ids.AccessTokenIssued);

            evt.DetailsFunc = () => new AccessTokenIssuedDetails
            {
                SubjectId = token.SubjectId ?? "no subject id",
                ClientId = token.ClientId,
                TokenType = token.Client.AccessTokenType,
                Lifetime = token.Lifetime,
                Scopes = token.Scopes,
                Claims = token.Claims.ToClaimsDictionary()
            };

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseIdentityTokenIssuedEventAsync(this IEventService events, Token token)
        {
            var evt = new Event<TokenIssuedDetailsBase>(
                EventConstants.Categories.TokenService,
                "Identity token issued",
                EventTypes.Information,
                EventConstants.Ids.IdentityTokenIssued);

            evt.DetailsFunc = () => new TokenIssuedDetailsBase
            {
                SubjectId = token.SubjectId,
                ClientId = token.ClientId,
                Lifetime = token.Lifetime,
                Claims = token.Claims.ToClaimsDictionary()
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseAuthorizationCodeIssuedEventAsync(this IEventService events, string id, AuthorizationCode code)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TokenService,
                "Authorization code issued",
                EventTypes.Information,
                EventConstants.Ids.AuthorizationCodeIssued);

            evt.DetailsFunc = () => new AuthorizationCodeDetails
            {
                HandleId = id,
                ClientId = code.ClientId,
                Scopes = code.Scopes,
                SubjectId = code.SubjectId,
                RedirectUri = code.RedirectUri,
                Lifetime = code.Client.AuthorizationCodeLifetime
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseRefreshTokenIssuedEventAsync(this IEventService events, string id, RefreshToken token)
        {
            var evt = new Event<RefreshTokenDetails>(
                EventConstants.Categories.TokenService,
                "Refresh token issued",
                EventTypes.Information,
                EventConstants.Ids.RefreshTokenIssued);

            evt.DetailsFunc = () => new RefreshTokenDetails
            {
                HandleId = id,
                ClientId = token.ClientId,
                Scopes = token.Scopes,
                SubjectId = token.SubjectId,
                Lifetime = token.LifeTime,
                Version = token.Version
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseSuccessfulRefreshTokenRefreshEventAsync(this IEventService events, string oldHandle, string newHandle, RefreshToken token)
        {
            var evt = new Event<RefreshTokenRefreshDetails>(
                EventConstants.Categories.TokenService,
                "Refresh token refresh success",
                EventTypes.Success,
                EventConstants.Ids.RefreshTokenRefreshedSuccess);

            evt.Details = new RefreshTokenRefreshDetails
            {
                OldHandle = oldHandle,
                NewHandle = newHandle,
                ClientId = token.ClientId,
                Lifetime = token.LifeTime
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseUnhandledExceptionEventAsync(this IEventService events, Exception exception)
        {
            var evt = new Event<object>(
                EventConstants.Categories.InternalError,
                "Unhandled exception",
                EventTypes.Error,
                EventConstants.Ids.UnhandledExceptionError, 
                exception.ToString());

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseSuccessfulEndpointEventAsync(this IEventService events, string endpointName)
        {
            var evt = new Event<EndpointDetail>(
                EventConstants.Categories.Endpoints,
                "Endpoint success",
                EventTypes.Success,
                EventConstants.Ids.EndpointSuccess,
                new EndpointDetail { EndpointName = endpointName });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailureEndpointEventAsync(this IEventService events, string endpointName, string error)
        {
            var evt = new Event<EndpointDetail>(
                 EventConstants.Categories.Endpoints,
                 "Endpoint failure",
                 EventTypes.Failure,
                 EventConstants.Ids.EndpointFailure,
                 new EndpointDetail { EndpointName = endpointName },
                 error);

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailedAuthorizationCodeRedeemedEventAsync(this IEventService events, Client client, string handle, string error)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TokenService,
                "Authorization code redeem failure",
                EventTypes.Failure,
                EventConstants.Ids.AuthorizationCodeRedeemedFailure,
                new AuthorizationCodeDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                },
                error);

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseSuccessAuthorizationCodeRedeemedEventAsync(this IEventService events, Client client, string handle)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TokenService,
                "Authorization code redeem success",
                EventTypes.Success,
                EventConstants.Ids.AuthorizationCodeRedeemedSuccess,
                new AuthorizationCodeDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailedRefreshTokenRefreshEventAsync(this IEventService events, Client client, string handle, string error)
        {
            var evt = new Event<RefreshTokenDetails>(
                EventConstants.Categories.TokenService,
                "Refresh token refresh failure",
                EventTypes.Failure,
                EventConstants.Ids.RefreshTokenRefreshedFailure,
                new RefreshTokenDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                },
                error);

            await events.RaiseAsync(evt);
        }

        public static Task RaiseSuccessRefreshTokenRefreshEventAsync(this IEventService events, Client client, string handle)
        {
            return Task.FromResult(0);
        }

        public static async Task RaiseNoCertificateConfiguredEventAsync(this IEventService events)
        {
            var evt = new Event<object>(
                EventConstants.Categories.Information,
                "No signing certificate configured",
                EventTypes.Information,
                EventConstants.Ids.NoSigningCertificateConfigured);

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseCertificatePrivateKeyNotAccessibleEventAsync(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.InternalError,
                "Signing certificate has no private key, or key is not accessible",
                EventTypes.Error,
                EventConstants.Ids.SigningCertificatePrivateKeyNotAccessible,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                },
                "Make sure the account running your application has access to the private key");

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseCertificateKeyLengthTooShortEventAsync(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.InternalError,
                "Signing certificate key length is less than 2048 bits.",
                EventTypes.Error,
                EventConstants.Ids.SigningCertificatePrivateKeyNotAccessible,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseCertificateExpiringSoonEventAsync(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.Information,
                "The signing certificate will expire in the next 30 days",
                EventTypes.Information,
                EventConstants.Ids.SigningCertificateExpiringSoon,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseCertificateValidatedEventAsync(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.Information,
                "Signing certificate validation success",
                EventTypes.Information,
                EventConstants.Ids.SigningCertificateValidated,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            await events.RaiseAsync(evt);
        }

        private static async Task RaiseEventAsync<T>(this IEventService events, Event<T> evt)
        {
            if (events == null) throw new ArgumentNullException("events");

            await events.RaiseAsync(evt);
        }
    }
}