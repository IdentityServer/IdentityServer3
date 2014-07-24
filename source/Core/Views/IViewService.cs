/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Views
{
    public interface IViewService
    {
        Task<Stream> Login(IDictionary<string, object> env, LoginViewModel model);
        Task<Stream> Logout(IDictionary<string, object> env, LogoutViewModel model);
        Task<Stream> LoggedOut(IDictionary<string, object> env, LoggedOutViewModel model);
        Task<Stream> Consent(IDictionary<string, object> env, ConsentViewModel model);
        Task<Stream> Error(IDictionary<string, object> env, ErrorViewModel model);
    }
}
