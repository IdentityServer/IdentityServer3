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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IViewService
    {
        Task<Stream> Login(IDictionary<string, object> env, LoginViewModel model, SignInMessage message);
        Task<Stream> Logout(IDictionary<string, object> env, LogoutViewModel model);
        Task<Stream> LoggedOut(IDictionary<string, object> env, LoggedOutViewModel model);
        Task<Stream> Consent(IDictionary<string, object> env, ConsentViewModel model);
        Task<Stream> ClientPermissions(IDictionary<string, object> env, ClientPermissionsViewModel model);
        Task<Stream> Error(IDictionary<string, object> env, ErrorViewModel model);
    }
}