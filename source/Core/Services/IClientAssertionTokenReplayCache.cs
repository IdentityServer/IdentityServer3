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

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// Interface that defines a simple cache for tacking replaying of security tokens.
    /// 
    /// </summary>
    public interface IClientAssertionTokenReplayCache
    {
        /// <summary>
        /// Try to add a securityToken.
        /// 
        /// </summary>
        /// <param name="securityToken">the security token to add.</param><param name="expiresOn">the time when security token expires.</param>
        /// <returns>
        /// true if the security token was successfully added.
        /// </returns>
        bool TryAdd(string securityToken, DateTime expiresOn);
        /// <summary>
        /// Try to find securityToken
        /// 
        /// </summary>
        /// <param name="securityToken">the security token to find.</param>
        /// <returns>
        /// true if the security token is found.
        /// </returns>
        bool TryFind(string securityToken);
    }
}
