using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Views;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Hosting;
using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [ErrorPageFilter]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    public class ClientPermissionsController : ApiController
    {
        IdentityServerOptions options;
        IViewService viewSvc;

        public ClientPermissionsController(IdentityServerOptions options, IViewService viewSvc)
        {
            this.options = options;
            this.viewSvc = viewSvc;
        }

        [Route(Constants.RoutePaths.Oidc.ClientPermissions, Name=Constants.RouteNames.Oidc.ClientPermissions)]
        public IHttpActionResult Get()
        {
            if (User == null || 
                User.Identity.IsAuthenticated == false)
            {
                return RedirectToLogin();
            }

            return RenderPermissionsPage();
        }

        private IHttpActionResult RedirectToLogin()
        {
            var message = new SignInMessage();

            var path = Url.Route(Constants.RouteNames.Oidc.ClientPermissions, null);
            var url = new Uri(Request.RequestUri, path);
            message.ReturnUrl = url.AbsoluteUri;

            return new LoginResult(message, Request.GetOwinContext().Environment, options);
        }

        private IHttpActionResult RenderPermissionsPage()
        {
            var env = Request.GetOwinEnvironment();
            var clients = GetClientPermissions();
            var vm = new ClientPermissionsViewModel()
            {
                SiteName = options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                CurrentUser = User.GetName(),
                LogoutUrl = Url.Link(Constants.RouteNames.Logout, null),
                AntiForgery = AntiForgeryTokenValidator.GetAntiForgeryHiddenInput(env),
                Clients = clients
            };
            return new ClientPermissionsActionResult(this.viewSvc, Request.GetOwinEnvironment(), vm);
        }

        private IEnumerable<ClientPermission> GetClientPermissions()
        {
            return null;
        }
    }
}
