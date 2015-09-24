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

using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default view service.
    /// </summary>
    public class DefaultViewService : IViewService
    {
        /// <summary>
        /// The login view
        /// </summary>
        public const string LoginView = "login";
        /// <summary>
        /// The logout view
        /// </summary>
        public const string LogoutView = "logout";
        /// <summary>
        /// The logged out view
        /// </summary>
        public const string LoggedOutView = "loggedOut";
        /// <summary>
        /// The consent view
        /// </summary>
        public const string ConsentView = "consent";
        /// <summary>
        /// The client permissions view
        /// </summary>
        public const string ClientPermissionsView = "permissions";
        /// <summary>
        /// The error view
        /// </summary>
        public const string ErrorView = "error";

        static readonly Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings()
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// The configuration
        /// </summary>
        protected readonly DefaultViewServiceOptions config;
        
        /// <summary>
        /// The view loader
        /// </summary>
        protected readonly IViewLoader viewLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewService"/> class.
        /// </summary>
        public DefaultViewService(DefaultViewServiceOptions config, IViewLoader viewLoader)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (viewLoader == null) throw new ArgumentNullException("viewLoader");

            this.config = config;
            this.viewLoader = viewLoader;
        }

        /// <summary>
        /// Loads the HTML for the login page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Login(LoginViewModel model, SignInMessage message)
        {
            return Render(model, LoginView);
        }

        /// <summary>
        /// Loads the HTML for the logout prompt page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Logout(LogoutViewModel model, SignOutMessage message)
        {
            return Render(model, LogoutView);
        }

        /// <summary>
        /// Loads the HTML for the logged out page informing the user that they have successfully logged out.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
            return Render(model, LoggedOutView);
        }

        /// <summary>
        /// Loads the HTML for the user consent page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="authorizeRequest">The validated authorize request.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest)
        {
            return Render(model, ConsentView);
        }

        /// <summary>
        /// Loads the HTML for the client permissions page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public Task<Stream> ClientPermissions(ClientPermissionsViewModel model)
        {
            return Render(model, ClientPermissionsView);
        }

        /// <summary>
        /// Loads the HTML for the error page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Error(ErrorViewModel model)
        {
            return Render(model, ErrorView);
        }

        /// <summary>
        /// Renders the specified page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        protected virtual Task<Stream> Render(CommonViewModel model, string page)
        {
            return Render(model, page, config.Stylesheets, config.Scripts);
        }

        /// <summary>
        /// Renders the specified page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="page">The page.</param>
        /// <param name="stylesheets">The stylesheets.</param>
        /// <param name="scripts">The scripts.</param>
        /// <returns></returns>
        protected virtual async Task<Stream> Render(CommonViewModel model, string page, IEnumerable<string> stylesheets, IEnumerable<string> scripts)
        {
            var data = BuildModel(model, page, stylesheets, scripts);

            string html = await LoadHtmlTemplate(page);
            html = FormatHtmlTemplate(html, data);

            return html.ToStream();
        }

        /// <summary>
        /// Loads the HTML template.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        protected virtual Task<string> LoadHtmlTemplate(string page)
        {
            return this.viewLoader.LoadAsync(page);
        }

        /// <summary>
        /// Formats the specified HTML template.
        /// </summary>
        /// <param name="htmlTemplate">The HTML template.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected string FormatHtmlTemplate(string htmlTemplate, object model)
        {
            return AssetManager.Format(htmlTemplate, model);
        }

        /// <summary>
        /// Builds the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="page">The page.</param>
        /// <param name="stylesheets">The stylesheets.</param>
        /// <param name="scripts">The scripts.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// model
        /// or
        /// stylesheets
        /// or
        /// scripts
        /// </exception>
        protected object BuildModel(CommonViewModel model, string page, IEnumerable<string> stylesheets, IEnumerable<string> scripts)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (stylesheets == null) throw new ArgumentNullException("stylesheets");
            if (scripts == null) throw new ArgumentNullException("scripts");
            
            var applicationPath = new Uri(model.SiteUrl).AbsolutePath;
            if (applicationPath.EndsWith("/")) applicationPath = applicationPath.Substring(0, applicationPath.Length - 1);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None, settings);

            var additionalStylesheets = BuildTags("<link href='{0}' rel='stylesheet'>", applicationPath, stylesheets);
            var additionalScripts = BuildTags("<script src='{0}'></script>", applicationPath, scripts);

            return new {
                siteName = Microsoft.Security.Application.Encoder.HtmlEncode(model.SiteName),
                applicationPath,
                model = Microsoft.Security.Application.Encoder.HtmlEncode(json),
                page,
                stylesheets = additionalStylesheets,
                scripts = additionalScripts
            };
        }

        string BuildTags(string tagFormat, string basePath, IEnumerable<string> values)
        {
            if (values == null || !values.Any()) return String.Empty;

            var sb = new StringBuilder();
            foreach (var value in values)
            {
                var path = value;
                if (path.StartsWith("~/"))
                {
                    path = basePath + path.Substring(1);
                }
                sb.AppendFormat(tagFormat, path);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
