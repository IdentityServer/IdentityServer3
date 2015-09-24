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
    /// Configures Content Security Policy (CSP) for HTML pages rendered by IdentityServer.
    /// </summary>
    public class CspOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CspOptions"/> class.
        /// </summary>
        public CspOptions()
        {
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether CSP is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }
        
        /// <summary>
        /// Allows additional script sources to be indicated.
        /// </summary>
        /// <value>
        /// The script source.
        /// </value>
        public string ScriptSrc { get; set; }
 
        /// <summary>
        /// Allows additional style sources to be indicated.
        /// </summary>
        /// <value>
        /// The style source.
        /// </value>
        public string StyleSrc { get; set; }

        /// <summary>
        /// Allows additional font sources to be indicated.
        /// </summary>
        /// <value>
        /// The font source.
        /// </value>
        public string FontSrc { get; set; }
        
        /// <summary>
        /// Allows additional connect sources to be indicated.
        /// </summary>
        /// <value>
        /// The connect source.
        /// </value>
        public string ConnectSrc { get; set; }
    }
}
