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

namespace Thinktecture.IdentityServer.Core.Models
{
    /// <summary>
    /// OpenID Connect scope types.
    /// </summary>
    public enum ScopeType
    {
        Identity,
        Resource
    }

    /// <summary>
    /// OpenID Connect flows.
    /// </summary>
    public enum Flows
    {
        AuthorizationCode,
        Implicit,
        Hybrid,
        ClientCredentials,
        ResourceOwner,
        Custom
    }

    /// <summary>
    /// OpenID Connect subject types.
    /// </summary>
    public enum SubjectTypes
    {
        Global,
        Ppid
    };

    /// <summary>
    /// OpenID Connect application types.
    /// </summary>
    public enum ApplicationTypes
    {
        Web,
        Native
    };

    /// <summary>
    /// Signing keys types.
    /// </summary>
    public enum SigningKeyTypes
    {
        Default,
        ClientSecret
    };

    /// <summary>
    /// Access token types.
    /// </summary>
    public enum AccessTokenType
    {
        Jwt,
        Reference
    }

    /// <summary>
    /// Token usage types.
    /// </summary>
    public enum TokenUsage
    {
        ReUse,
        OneTimeOnly
    }

    /// <summary>
    /// Token expiration types.
    /// </summary>
    public enum TokenExpiration
    {
        Sliding,
        Absolute
    }
}
