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
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    internal static class IEventServiceExtensions
    {
        public static void RaisePreLoginSuccessEvent(this IEventService events, string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new PreLoginEvent(EventTypes.Success)
            {
                SubjectId = authResult.User.GetSubjectId(),
                Name = authResult.User.Identity.Name,
                SignInId = signInMessageId,
                SignInMessage = signInMessage,
                PartialLogin = authResult.IsPartialSignIn
            };

            events.RaiseEvent(evt);
        }

        public static void RaisePreLoginFailureEvent(this IEventService events, string signInMessageId, SignInMessage signInMessage, string details)
        {
            var evt = new PreLoginEvent(EventTypes.Failure)
            {
                SignInId = signInMessageId,
                SignInMessage = signInMessage,
                Details = details
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseLocalLoginSuccessEvent(this IEventService events, string username, string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new LocalLoginEvent(EventTypes.Success)
            {
                SubjectId = authResult.User.GetSubjectId(),
                Name = authResult.User.Identity.Name,
                SignInId = signInMessageId,
                SignInMessage = signInMessage,
                LoginUserName = username,
                PartialLogin = authResult.IsPartialSignIn
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseLocalLoginFailureEvent(this IEventService events, string username, string signInMessageId, SignInMessage signInMessage, string details)
        {
            var evt = new LocalLoginEvent(EventTypes.Failure)
            {
                SignInId = signInMessageId,
                SignInMessage = signInMessage,
                LoginUserName = username,
                Details = details
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseExternalLoginSuccessEvent(this IEventService events, ExternalIdentity externalIdentity, string signInMessageId, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new ExternalLoginEvent(EventTypes.Success)
            {
                Provider = externalIdentity.Provider,
                ProviderId = externalIdentity.ProviderId,
                SubjectId = authResult.User.GetSubjectId(),
                Name = authResult.User.Identity.Name,
                SignInId = signInMessageId,
                SignInMessage = signInMessage,
                PartialLogin = authResult.IsPartialSignIn
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseExternalLoginFailureEvent(this IEventService events, ExternalIdentity externalIdentity, string signInMessageId, SignInMessage signInMessage, string details)
        {
            var evt = new ExternalLoginEvent(EventTypes.Failure)
            {
                Provider = externalIdentity.Provider,
                ProviderId = externalIdentity.ProviderId,
                SignInId = signInMessageId,
                SignInMessage = signInMessage,
                Details = details
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseExternalLoginErrorEvent(this IEventService events, string details)
        {
            var evt = new ExternalLoginEvent(EventTypes.Error)
            {
                Details = details
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseSuccessfulResourceOwnerFlowAuthenticationEvent(this IEventService events, string userName, string subjectId, SignInMessage message)
        {
            var evt = new ResourceOwnerPasswordFlowAuthenticationEvent(EventTypes.Success)
            {
                LoginUserName = userName,
                SubjectId = subjectId,
                SignInMessage = message
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseFailedResourceOwnerFlowAuthenticationEvent(this IEventService events, string userName, SignInMessage message)
        {
            var evt = new ResourceOwnerPasswordFlowAuthenticationEvent(EventTypes.Failure)
            {
                LoginUserName = userName,
                SignInMessage = message
            };

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

        public static void RaisePartialLoginCompleteEvent(this IEventService events, ClaimsIdentity subject, string signInMessageId, SignInMessage signInMessage)
        {
            var evt = new PartialLoginCompleteEvent()
            {
                SubjectId = subject.GetSubjectId(),
                Name = subject.Name,
                SignInId = signInMessageId,
                SignInMessage = signInMessage
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseLogoutEvent(this IEventService events, ClaimsPrincipal subject, SignOutMessage signOutMessage)
        {
            var evt = new LogoutEvent()
            {
                SubjectId = subject.GetSubjectId(),
                Name = subject.Identity.Name,
                SignOutMessage = signOutMessage
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseTokenIssuedEvent(this IEventService events, Token token)
        {
            if (token.Type == Constants.TokenTypes.AccessToken)
            {
                events.RaiseAccessTokenIssuedEvent(token);
            }
            else if (token.Type == Constants.TokenTypes.IdentityToken)
            {
                events.RaiseIdentityTokenIssuedEvent(token);
            }
        }

        public static void RaiseAccessTokenIssuedEvent(this IEventService events, Token token)
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
            
            events.Raise(evt);
        }

        public static void RaiseIdentityTokenIssuedEvent(this IEventService events, Token token)
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

            events.RaiseEvent(evt);
        }

        public static void RaiseAuthorizationCodeIssuedEvent(this IEventService events, string id, AuthorizationCode code)
        {
            var evt = new Event<AuthorizationCodeIssuedDetailsBase>(
                EventConstants.Categories.TokenService,
                "Authorization code issued",
                EventTypes.Information,
                EventConstants.Ids.AuthorizationCodeIssued);

            evt.DetailsFunc = () => new AuthorizationCodeIssuedDetailsBase
            {
                HandleId = id,
                ClientId = code.ClientId,
                Scopes = code.Scopes,
                SubjectId = code.SubjectId,
                RedirectUri = code.RedirectUri
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseRefreshTokenIssuedEvent(this IEventService events, string id, RefreshToken token)
        {
            var evt = new Event<RefreshTokenIssuedDetails>(
                EventConstants.Categories.TokenService,
                "Refresh token issued",
                EventTypes.Information,
                EventConstants.Ids.RefreshTokenIssued);

            evt.DetailsFunc = () => new RefreshTokenIssuedDetails
            {
                HandleId = id,
                ClientId = token.ClientId,
                Scopes = token.Scopes,
                SubjectId = token.SubjectId,
                Lifetime = token.LifeTime,
                Version = token.Version
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseSuccessfulRefreshTokenRefreshEvent(this IEventService events, string oldHandle, string newHandle, RefreshToken token)
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

            events.RaiseEvent(evt);
        }

        public static void RaiseUnhandledExceptionEvent(this IEventService events, Exception exception)
        {
            var evt = new UnhandledExceptionEvent
            {
                Details = exception.ToString()
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseSuccessfulEndpointEvent(this IEventService events, string endpointName)
        {
            var evt = new Event<EndpointDetail>(
                EventConstants.Categories.Endpoints,
                "Endpoint success",
                EventTypes.Success,
                EventConstants.Ids.EndpointSuccess,
                new EndpointDetail { EndpointName = endpointName });

            events.Raise(evt);
        }

        public static void RaiseFailureEndpointEvent(this IEventService events, string endpointName, string error)
        {
            var evt = new Event<EndpointDetail>(
                 EventConstants.Categories.Endpoints,
                 "Endpoint failure",
                 EventTypes.Failure,
                 EventConstants.Ids.EndpointFailure,
                 new EndpointDetail { EndpointName = endpointName });

            events.Raise(evt);
        }

        private static void RaiseEvent<T>(this IEventService events, Event<T> evt)
        {
            if (events == null) throw new ArgumentNullException("events");

            events.Raise(evt);
        }
    }
}