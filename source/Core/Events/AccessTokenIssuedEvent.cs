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

using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Events
{
    public class AccessTokenIssuedEvent : EventBase
    {
        public AccessTokenIssuedEvent() : base(EventConstants.Categories.TokenService)
        {
            EventType = EventType.Success;
            Id = EventConstants.Ids.AccessTokenIssued;
        }

        public string SubjectId { get; set; }
        public string ClientId { get; set; }
        public AccessTokenType TokenType { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public int Lifetime { get; set; }
    }
}