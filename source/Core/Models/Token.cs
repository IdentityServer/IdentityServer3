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
using System.Linq;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Models
{
    public interface ITokenMetadata
    {
        string SubjectId { get; }
        string ClientId { get; }
        IEnumerable<string> Scopes { get; }
    }
    
    public class InMemoryTokenMetadata : ITokenMetadata
    {
        public string SubjectId { get; set; }
        public string ClientId { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }

    public class Token : ITokenMetadata
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public DateTime CreationTime { get; set; }
        public int Lifetime { get; set; }
        public string Type { get; set; }
        public Client Client { get; set; }

        public List<Claim> Claims { get; set; }

        public Token()
        {
            Type = Constants.TokenTypes.AccessToken;
            CreationTime = DateTime.UtcNow;
        }

        public Token(string tokenType)
        {
            Type = tokenType;
            CreationTime = DateTime.UtcNow;
        }

        public string SubjectId
        {
            get
            {
                return Claims.Where(x => x.Type == Constants.ClaimTypes.Subject).Select(x => x.Value).SingleOrDefault();
            }
        }

        public string ClientId
        {
            get
            {
                return Client.ClientId;
            }
        }

        public IEnumerable<string> Scopes
        {
            get { return Claims.Where(x => x.Type == Constants.ClaimTypes.Scope).Select(x => x.Value); }
        }
    }
}