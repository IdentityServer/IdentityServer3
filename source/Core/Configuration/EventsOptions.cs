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

namespace IdentityServer3.Core.Configuration
{
    /// <summary>
    /// Configures events
    /// </summary>
    public class EventsOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to raise success events.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success event should be raised; otherwise, <c>false</c>.
        /// </value>
        public bool RaiseSuccessEvents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to raise failure events.
        /// </summary>
        /// <value>
        ///   <c>true</c> if failure events should be raised; otherwise, <c>false</c>.
        /// </value>
        public bool RaiseFailureEvents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to raise information events.
        /// </summary>
        /// <value>
        /// <c>true</c> if information events should be raised; otherwise, <c>false</c>.
        /// </value>
        public bool RaiseInformationEvents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to raise error events.
        /// </summary>
        /// <value>
        ///   <c>true</c> if error events should be raised; otherwise, <c>false</c>.
        /// </value>
        public bool RaiseErrorEvents { get; set; }
    }
}