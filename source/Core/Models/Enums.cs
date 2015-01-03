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
        /// <summary>
        /// Scope representing identity data (e.g. profile or email)
        /// </summary>
        Identity,

        /// <summary>
        /// Scope representing a resource (e.g. a web api)
        /// </summary>
        Resource
    }

    /// <summary>
    /// OpenID Connect flows.
    /// </summary>
    public enum Flows
    {
        /// <summary>
        /// authorization code flow
        /// </summary>
        AuthorizationCode,

        /// <summary>
        /// implicit flow
        /// </summary>
        Implicit,

        /// <summary>
        /// hybrid flow
        /// </summary>
        Hybrid,

        /// <summary>
        /// client credentials flow
        /// </summary>
        ClientCredentials,

        /// <summary>
        /// resource owner password credential flow
        /// </summary>
        ResourceOwner,

        /// <summary>
        /// custom grant
        /// </summary>
        Custom
    }

    /// <summary>
    /// OpenID Connect subject types.
    /// </summary>
    public enum SubjectTypes
    {
        /// <summary>
        /// global - use the native subject id
        /// </summary>
        Global,

        /// <summary>
        /// ppid - scope the subject id to the client
        /// </summary>
        Ppid
    };

    /// <summary>
    /// Signing keys types.
    /// </summary>
    public enum SigningKeyTypes
    {
        /// <summary>
        /// use the default signing certificate
        /// </summary>
        SigningCertificate,
        
        /// <summary>
        /// use the client secret
        /// </summary>
        ClientSecret
    };

    /// <summary>
    /// Access token types.
    /// </summary>
    public enum AccessTokenType
    {
        /// <summary>
        /// Self-contained Json Web Token
        /// </summary>
        Jwt,

        /// <summary>
        /// Reference token
        /// </summary>
        Reference
    }

    /// <summary>
    /// Token usage types.
    /// </summary>
    public enum TokenUsage
    {
        /// <summary>
        /// Re-use the refresh token handle
        /// </summary>
        ReUse,

        /// <summary>
        /// Issue a new refresh token handle every time
        /// </summary>
        OneTimeOnly
    }

    /// <summary>
    /// Token expiration types.
    /// </summary>
    public enum TokenExpiration
    {
        /// <summary>
        /// Sliding token expiration
        /// </summary>
        Sliding,

        /// <summary>
        /// Absolute token expiration
        /// </summary>
        Absolute
    }
}