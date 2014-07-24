/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Views.Embedded.Assets;

namespace Thinktecture.IdentityServer.Core.Views.Embedded
{
    public class EmbeddedAssetsViewService : IViewService
    {
        public Task<System.IO.Stream> Login(IDictionary<string, object> env, LoginViewModel model)
        {
            return Render(model, "login");
        }

        public Task<System.IO.Stream> Logout(IDictionary<string, object> env, LogoutViewModel model)
        {
            return Render(model, "logout");
        }

        public Task<System.IO.Stream> LoggedOut(IDictionary<string, object> env, LoggedOutViewModel model)
        {
            return Render(model, "loggedOut");
        }

        public Task<System.IO.Stream> Consent(IDictionary<string, object> env, ConsentViewModel model)
        {
            return Render(model, "consent");
        }

        public Task<System.IO.Stream> Error(IDictionary<string, object> env, ErrorViewModel model)
        {
            return Render(model, "error");
        }

        Task<System.IO.Stream> Render(CommonViewModel model, string page)
        {
            var html = AssetManager.GetLayoutHtml(model, page);
            return Task.FromResult(StringToStream(html));
        }

        Stream StringToStream(string s)
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
