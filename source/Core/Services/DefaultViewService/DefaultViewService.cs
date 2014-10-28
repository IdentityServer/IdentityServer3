/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
    public class DefaultViewService : IViewService
    {
        private readonly DefaultViewServiceConfiguration config;

        public DefaultViewService()
            : this(DefaultViewServiceConfiguration.Default)
        {
        }

        public DefaultViewService(DefaultViewServiceConfiguration config)
        {
            if (config == null) throw new ArgumentNullException("config");
            
            this.config = config;
        }

        public virtual Task<Stream> Login(IDictionary<string, object> env, LoginViewModel model, SignInMessage message)
        {
            return Render(model, "login");
        }

        public virtual Task<Stream> Logout(IDictionary<string, object> env, LogoutViewModel model)
        {
            return Render(model, "logout");
        }

        public virtual Task<System.IO.Stream> LoggedOut(IDictionary<string, object> env, LoggedOutViewModel model)
        {
            return Render(model, "loggedOut");
        }

        public virtual Task<Stream> Consent(IDictionary<string, object> env, ConsentViewModel model)
        {
            return Render(model, "consent");
        }

        public Task<Stream> ClientPermissions(IDictionary<string, object> env, ClientPermissionsViewModel model)
        {
            return Render(model, "permissions");
        }

        public virtual Task<System.IO.Stream> Error(IDictionary<string, object> env, ErrorViewModel model)
        {
            return Render(model, "error");
        }

        protected virtual Task<Stream> Render(CommonViewModel model, string page)
        {
            string html = this.config.GetLoader().Load(page);

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

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, 
                Newtonsoft.Json.Formatting.None, 
                new Newtonsoft.Json.JsonSerializerSettings() { 
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() 
                }
            );

            var additionalStylesheets = BuildTags("<link href='{0}' rel='stylesheet'>", applicationPath, stylesheets);
            var additionalScripts = BuildTags("<script src='{0}'></script>", applicationPath, scripts);

            return new {
                siteName = model.SiteName,
                applicationPath,
                model = json,
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
