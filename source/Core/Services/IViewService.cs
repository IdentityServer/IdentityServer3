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

using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// Models loading the necessary HTML pages displayed by IdentityServer.
    /// </summary>
    public interface IViewService
    {
        /// <summary>
        /// Loads the HTML for the login page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>Stream for the HTML</returns>
        Task<Stream> Login(LoginViewModel model, SignInMessage message);

        /// <summary>
        /// Loads the HTML for the logout prompt page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        Task<Stream> Logout(LogoutViewModel model, SignOutMessage message);

        /// <summary>
        /// Loads the HTML for the logged out page informing the user that they have successfully logged out.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message);

        /// <summary>
        /// Loads the HTML for the user consent page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="authorizeRequest">The validated authorize request.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest);

        /// <summary>
        /// Loads the HTML for the client permissions page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Stream for the HTML</returns>
        Task<Stream> ClientPermissions(ClientPermissionsViewModel model);

        /// <summary>
        /// Loads the HTML for the error page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Stream for the HTML</returns>
        Task<Stream> Error(ErrorViewModel model);
    }
}