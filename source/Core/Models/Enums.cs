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

namespace IdentityServer3.Core.Models
{
    /// <summary>
    /// OpenID Connect scope types.
    /// </summary>
    public enum ScopeType
    {
        /// <summary>
        /// Scope representing identity data (e.g. profile or email)
        /// </summary>
        Identity = 0,

        /// <summary>
        /// Scope representing a resource (e.g. a web api)
        /// </summary>
        Resource = 1
    }

    /// <summary>
    /// OpenID Connect flows.
    /// </summary>
    public enum Flows
    {
        /// <summary>
        /// authorization code flow
        /// </summary>
        AuthorizationCode = 0,

        /// <summary>
        /// implicit flow
        /// </summary>
        Implicit = 1,

        /// <summary>
        /// hybrid flow
        /// </summary>
        Hybrid = 2,

        /// <summary>
        /// client credentials flow
        /// </summary>
        ClientCredentials = 3,

        /// <summary>
        /// resource owner password credential flow
        /// </summary>
        ResourceOwner = 4,

        /// <summary>
        /// custom grant
        /// </summary>
        Custom = 5
    }

    /// <summary>
    /// OpenID Connect subject types.
    /// </summary>
    public enum SubjectTypes
    {
        /// <summary>
        /// global - use the native subject id
        /// </summary>
        Global = 0,

        /// <summary>
        /// ppid - scope the subject id to the client
        /// </summary>
        Ppid = 1
    };

    /// <summary>
    /// Access token types.
    /// </summary>
    public enum AccessTokenType
    {
        /// <summary>
        /// Self-contained Json Web Token
        /// </summary>
        Jwt = 0,

        /// <summary>
        /// Reference token
        /// </summary>
        Reference = 1
    }

    /// <summary>
    /// Token usage types.
    /// </summary>
    public enum TokenUsage
    {
        /// <summary>
        /// Re-use the refresh token handle
        /// </summary>
        ReUse = 0,

        /// <summary>
        /// Issue a new refresh token handle every time
        /// </summary>
        OneTimeOnly = 1
    }

    /// <summary>
    /// Token expiration types.
    /// </summary>
    public enum TokenExpiration
    {
        /// <summary>
        /// Sliding token expiration
        /// </summary>
        Sliding = 0,

        /// <summary>
        /// Absolute token expiration
        /// </summary>
        Absolute = 1
    }
}