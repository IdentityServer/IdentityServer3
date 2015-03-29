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

using Microsoft.Owin;
using System;
using System.Collections.Generic;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// Container for the OWIN environment.
    /// </summary>
    public class OwinEnvironmentService
    {
        readonly IDictionary<string, object> _environment;

        internal OwinEnvironmentService(IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            _environment = context.Environment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinEnvironmentService"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public OwinEnvironmentService(IDictionary<string, object> environment)
        {
            if (environment == null) throw new ArgumentNullException("environment");

            _environment = environment;
        }

        /// <summary>
        /// Gets the OWIN environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        public IDictionary<string, object> Environment 
        { 
            get
            {
                return _environment;
            }
        }
    }
}