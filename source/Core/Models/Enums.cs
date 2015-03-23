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

namespace Thinktecture.IdentityServer.Core.Models
{
    /// <summary>
    /// OpenID Connect scope types.
    /// </summary>
    public enum ScopeType
    {
        /// <summary>
        /// Scope representing identity data (e.g. profile or email)
        /// </summary>
        IDENTITY = 0,

        /// <summary>
        /// Scope representing a resource (e.g. a web api)
        /// </summary>
        RESOURCE = 1
    }

    /// <summary>
    /// OpenID Connect flows.
    /// </summary>
    public enum Flows
    {
        /// <summary>
        /// authorization code flow
        /// </summary>
        AUTHORIZATION_CODE = 0,

        /// <summary>
        /// implicit flow
        /// </summary>
        IMPLICIT = 1,

        /// <summary>
        /// hybrid flow
        /// </summary>
        HYBRID = 2,

        /// <summary>
        /// client credentials flow
        /// </summary>
        CLIENT_CREDENTIALS = 3,

        /// <summary>
        /// resource owner password credential flow
        /// </summary>
        RESOURCE_OWNER = 4,

        /// <summary>
        /// custom grant
        /// </summary>
        CUSTOM = 5
    }

    /// <summary>
    /// OpenID Connect subject types.
    /// </summary>
    public enum SubjectTypes
    {
        /// <summary>
        /// global - use the native subject id
        /// </summary>
        GLOBAL = 0,

        /// <summary>
        /// ppid - scope the subject id to the client
        /// </summary>
        PPID = 1
    };

    /// <summary>
    /// Access token types.
    /// </summary>
    public enum AccessTokenType
    {
        /// <summary>
        /// Self-contained Json Web Token
        /// </summary>
        JWT = 0,

        /// <summary>
        /// Reference token
        /// </summary>
        REFERENCE = 1
    }

    /// <summary>
    /// Token usage types.
    /// </summary>
    public enum TokenUsage
    {
        /// <summary>
        /// Re-use the refresh token handle
        /// </summary>
        RE_USE = 0,

        /// <summary>
        /// Issue a new refresh token handle every time
        /// </summary>
        ONE_TIME_ONLY = 1
    }

    /// <summary>
    /// Token expiration types.
    /// </summary>
    public enum TokenExpiration
    {
        /// <summary>
        /// Sliding token expiration
        /// </summary>
        SLIDING = 0,

        /// <summary>
        /// Absolute token expiration
        /// </summary>
        ABSOLUTE = 1
    }
}