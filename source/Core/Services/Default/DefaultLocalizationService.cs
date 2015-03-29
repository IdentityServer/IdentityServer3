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

using IdentityServer3.Core.Resources;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default localization services. Uses embedded resource files for strings.
    /// The category is used to identify which resource file from which to read.
    /// </summary>
    public class DefaultLocalizationService : ILocalizationService
    {
        /// <summary>
        /// Gets a localized string based upon the string's category and identifier.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual string GetString(string category, string id)
        {
            switch(category)
            {
                case Constants.LocalizationCategories.Messages:
                    return Messages.ResourceManager.GetString(id);
                case Constants.LocalizationCategories.Events:
                    return Resources.Events.ResourceManager.GetString(id);
                case Constants.LocalizationCategories.Scopes:
                    return Scopes.ResourceManager.GetString(id);
            }
            
            return null;
        }
    }
}
