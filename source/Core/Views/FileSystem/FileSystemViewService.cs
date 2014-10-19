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