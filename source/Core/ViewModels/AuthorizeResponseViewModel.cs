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

namespace IdentityServer3.Core.ViewModels
{
    /// <summary>
    /// Models the data needed to render the authorize response page.
    /// </summary>
    public class AuthorizeResponseViewModel : CommonViewModel
    {
        /// <summary>
        /// The action URI for the response form.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        public string ResponseFormUri { get; set; }

        /// <summary>
        /// The input elements for the response form.
        /// </summary>
        /// <value>
        /// The HTML.
        /// </value>
        public string ResponseFormFields { get; set; }
    }
}
