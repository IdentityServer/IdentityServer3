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

namespace Thinktecture.IdentityServer.Core.Events.Base
{
    /// <summary>
    /// Indicates if the event is a success or fail event.
    /// </summary>
    public enum EventTypes
    {
        /// <summary>
        /// Success event
        /// </summary>
        SUCCESS = 1,

        /// <summary>
        /// Failure event
        /// </summary>
        FAILURE = 2,

        /// <summary>
        /// Information event
        /// </summary>
        INFORMATION = 3,
        
        /// <summary>
        /// Error event
        /// </summary>
        ERROR = 4,
    }
}