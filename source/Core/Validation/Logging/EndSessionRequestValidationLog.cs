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
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Validation.Logging
{
    class EndSessionRequestValidationLog
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string SubjectId { get; set; }

        public string PostLogOutUri { get; set; }
        public string State { get; set; }

        public Dictionary<string, string> Raw { get; set; }

        public EndSessionRequestValidationLog(ValidatedEndSessionRequest request)
        {
            if (request.Options.DiagnosticsOptions.IncludeSensitiveDataInLogs)
            {
                Raw = request.Raw.ToDictionary();
            }

            SubjectId = "unknown";
            if (request.Subject != null)
            {
                var subjectClaim = request.Subject.FindFirst(Constants.ClaimTypes.Subject);
                if (subjectClaim != null)
                {
                    SubjectId = subjectClaim.Value;
                }
            }

            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;
            }

            PostLogOutUri = request.PostLogOutUri;
            State = request.State;
        }
    }
}