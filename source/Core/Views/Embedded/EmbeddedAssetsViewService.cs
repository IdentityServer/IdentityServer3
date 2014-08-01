/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Views.Embedded.Assets;

namespace Thinktecture.IdentityServer.Core.Views.Embedded
{
    public class EmbeddedAssetsViewService : IViewService
    {
        private EmbeddedAssetsViewServiceConfiguration config;

        public EmbeddedAssetsViewService()
        {
            config = new EmbeddedAssetsViewServiceConfiguration();
        }

        public EmbeddedAssetsViewService(EmbeddedAssetsViewServiceConfiguration config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        public virtual Task<System.IO.Stream> Login(IDictionary<string, object> env, LoginViewModel model)
        {
            return Render(model, "login");
        }

        public virtual Task<System.IO.Stream> Logout(IDictionary<string, object> env, LogoutViewModel model)
        {
            return Render(model, "logout");
        }

        public virtual Task<System.IO.Stream> LoggedOut(IDictionary<string, object> env, LoggedOutViewModel model)
        {
            return Render(model, "loggedOut");
        }

        public virtual Task<System.IO.Stream> Consent(IDictionary<string, object> env, ConsentViewModel model)
        {
            return Render(model, "consent");
        }

        public virtual Task<System.IO.Stream> Error(IDictionary<string, object> env, ErrorViewModel model)
        {
            return Render(model, "error");
        }

        protected virtual Task<System.IO.Stream> Render(CommonViewModel model, string page)
        {
            string html = AssetManager.GetLayoutHtml(model, page, config.Stylesheets, config.Scripts);
            return Task.FromResult(StringToStream(html));
        }

        protected Stream StringToStream(string s)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(s);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
