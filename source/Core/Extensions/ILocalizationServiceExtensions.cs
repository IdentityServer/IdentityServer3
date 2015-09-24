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

using IdentityServer3.Core.Services;
using System;

namespace IdentityServer3.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IdentityServer3.Core.Services.ILocalizationService"/>
    /// </summary>
    public static class ILocalizationServiceExtensions
    {
        /// <summary>
        /// Gets a localized string for the message category.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetMessage(this ILocalizationService localization, string id)
        {
            if (localization == null) throw new ArgumentNullException("localization");

            return localization.GetString(Constants.LocalizationCategories.Messages, id);
        }

        /// <summary>
        /// Gets a localized string for the event category.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetEvent(this ILocalizationService localization, string id)
        {
            if (localization == null) throw new ArgumentNullException("localization");

            return localization.GetString(Constants.LocalizationCategories.Events, id);
        }

        /// <summary>
        /// Gets a localized scope display name.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetScopeDisplayName(this ILocalizationService localization, string scope)
        {
            if (localization == null) throw new ArgumentNullException("localization");
            
            return localization.GetString(Constants.LocalizationCategories.Scopes, scope + Constants.ScopeDisplayNameSuffix);
        }

        /// <summary>
        /// Gets a localized scope description.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetScopeDescription(this ILocalizationService localization, string scope)
        {
            if (localization == null) throw new ArgumentNullException("localization");
            
            return localization.GetString(Constants.LocalizationCategories.Scopes, scope + Constants.ScopeDescriptionSuffix);
        }
    }
}
