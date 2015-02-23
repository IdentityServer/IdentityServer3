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

namespace Thinktecture.IdentityServer.Core.Configuration
{
    /// <summary>
    /// Configures logging within IdentityServer.
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingOptions"/> class.
        /// </summary>
        public LoggingOptions()
        {
            EnableWebApiDiagnostics = false;
            WebApiDiagnosticsIsVerbose = false;
            EnableHttpLogging = false;
            IncludeSensitiveDataInLogs = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether web API diagnostics should be enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if web API diagnostics should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableWebApiDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether web API diagnostics logging should be set to verbose.
        /// </summary>
        /// <value>
        /// <c>true</c> if web API diagnostics logging shozld be verbose; otherwise, <c>false</c>.
        /// </value>
        public bool WebApiDiagnosticsIsVerbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTTP request/response logging is enabled
        /// </summary>
        /// <value>
        ///   <c>true</c> if HTTP logging is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableHttpLogging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include (potentially) personally identifiable information or other sensitive data in logs.
        /// </summary>
        /// <value>
        ///   <c>true</c> if PII data should be included in logs; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeSensitiveDataInLogs { get; set; }
    }
}