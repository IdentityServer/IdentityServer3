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

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Events;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    internal static class IEventServiceExtensions
    {
        public static void RaiseLocalLoginSuccessEvent(this IEventService events, string username, SignInMessage signInMessage, AuthenticateResult authResult)
        {
            var evt = new LocalAuthenticationEvent(EventType.Success)
            {
                Id = EventConstants.Ids.LocalLogin,
                EventType = EventType.Success,
                Message = Resources.Events.LocalLoginSuccess,
                SubjectId = authResult.User.GetSubjectId(),
                SignInMessage = signInMessage,
                LoginUserName = username
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
            var subjectId = "none";

            if (!string.IsNullOrWhiteSpace(token.SubjectId))
            {
                subjectId = token.SubjectId;
            }

            var evt = new AccessTokenIssuedEvent
            {
                SubjectId = subjectId,
                ClientId = token.ClientId,
                TokenType = token.Client.AccessTokenType,
                Lifetime = token.Lifetime,
                Scopes = token.Scopes,
                Claims = token.Claims.ToClaimsDictionary()
            };

            events.RaiseEvent(evt);
        }

        public static void RaiseIdentityTokenIssuedEvent(this IEventService events, Token token)
        {
            var evt = new IdentityTokenIssuedEvent
            {
                SubjectId = token.SubjectId,
                ClientId = token.ClientId,
                Lifetime = token.Lifetime,
                Claims = token.Claims.ToClaimsDictionary()
            };

            events.RaiseEvent(evt);
        }

        private static void RaiseEvent(this IEventService events, EventBase evt)
        {
            if (events == null) throw new ArgumentNullException("events");

            events.Raise(evt);
        }
    }
}