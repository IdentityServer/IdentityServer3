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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default view service.
    /// </summary>
    public class DefaultViewService : IViewService
    {
        static readonly Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings()
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        private readonly DefaultViewServiceOptions config;
        private readonly IViewLoader viewLoader;

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
            return Render(model, "login");
        }

        /// <summary>
        /// Loads the HTML for the logout prompt page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Logout(LogoutViewModel model)
        {
            return Render(model, "logout");
        }

        /// <summary>
        /// Loads the HTML for the logged out page informing the user that they have successfully logged out.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> LoggedOut(LoggedOutViewModel model)
        {
            return Render(model, "loggedOut");
        }

        /// <summary>
        /// Loads the HTML for the user consent page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Stream for the HTML
        /// </returns>
        public virtual Task<Stream> Consent(ConsentViewModel model)
        {
            return Render(model, "consent");
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
            return Render(model, "permissions");
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
            return Render(model, "error");
        }

        /// <summary>
        /// Renders the specified page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        protected virtual Task<Stream> Render(CommonViewModel model, string page)
        {
            string html = this.viewLoader.Load(page);

            var data = BuildModel(model, page, config.Stylesheets, config.Scripts);
            html = AssetManager.Format(html, data);
            
            return Task.FromResult(html.ToStream());
        }

        object BuildModel(CommonViewModel model, string page, ICollection<string> stylesheets, ICollection<string> scripts)
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
