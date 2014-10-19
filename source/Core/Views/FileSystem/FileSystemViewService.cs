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
using Owin;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Views.FileSystem
{
    public class FileSystemViewService : IViewService
    {
        string directory;
        IViewService fallback;

        public FileSystemViewService(string directory, IViewService fallback)
        {
            this.directory = directory;
            this.fallback = fallback;
        }

        public System.Threading.Tasks.Task<System.IO.Stream> Login(System.Collections.Generic.IDictionary<string, object> env, LoginViewModel model, Authentication.SignInMessage message)
        {
            throw new System.NotImplementedException();
        }

        public System.Threading.Tasks.Task<System.IO.Stream> Logout(System.Collections.Generic.IDictionary<string, object> env, LogoutViewModel model)
        {
            throw new System.NotImplementedException();
        }

        public System.Threading.Tasks.Task<System.IO.Stream> LoggedOut(System.Collections.Generic.IDictionary<string, object> env, LoggedOutViewModel model)
        {
            throw new System.NotImplementedException();
        }

        public System.Threading.Tasks.Task<System.IO.Stream> Consent(System.Collections.Generic.IDictionary<string, object> env, ConsentViewModel model)
        {
            throw new System.NotImplementedException();
        }

        public Task<Stream> ClientPermissions(IDictionary<string, object> env, ClientPermissionsViewModel model)
        {
            throw new System.NotImplementedException();
        }
 
        public System.Threading.Tasks.Task<System.IO.Stream> Error(IDictionary<string, object> env, ErrorViewModel model)
        {
            throw new System.NotImplementedException();
        }

        protected virtual Task<System.IO.Stream> Render(string name, IDictionary<string, object> env, object model)
        {
            return Task.FromResult<Stream>(null);
        }

        //string 

        //bool IsFile(string name)
        //{
        //    File.Exists
        //}
   }
}