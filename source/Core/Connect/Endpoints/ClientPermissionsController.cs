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
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [ErrorPageFilter]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    public class ClientPermissionsController : ApiController
    {
        IClientPermissionsService clientPermissionsService;
        IdentityServerOptions options;
        IViewService viewSvc;

        public ClientPermissionsController(IClientPermissionsService clientPermissionsService, IdentityServerOptions options, IViewService viewSvc)
        {
            this.clientPermissionsService = clientPermissionsService;
            this.options = options;
            this.viewSvc = viewSvc;
        }

        [Route(Constants.RoutePaths.Oidc.ClientPermissions)]
        [HttpGet]
        public async Task<IHttpActionResult> ShowPermissions()
        {
            if (options.ClientPermissionsEndpoint.IsEnabled == false)
            {
                return NotFound();
            }

            if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
            {
                return RedirectToLogin();
            }

            return await RenderPermissionsPage();
        }

        [Route(Constants.RoutePaths.Oidc.ClientPermissions, Name = Constants.RouteNames.Oidc.ClientPermissions)]
        [HttpPost]
        public async Task<IHttpActionResult> RevokePermission(RevokeClientPermission model)
        {
            if (options.ClientPermissionsEndpoint.IsEnabled == false)
            {
                return NotFound();
            }
            
            if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
            {
                return RedirectToLogin();
            }

            if (model == null || ModelState.IsValid == false)
            {
                var error = ModelState.Where(x => x.Value.Errors.Any()).Select(x => x.Value.Errors.First().ErrorMessage).First();
                return await RenderPermissionsPage(error);
            }

            await this.clientPermissionsService.RevokeClientPermissionsAsync(User.GetSubjectId(), model.ClientId);

            return RedirectToRoute(Constants.RouteNames.Oidc.ClientPermissions, null);
        }

        private IHttpActionResult RedirectToLogin()
        {
            var message = new SignInMessage();

            var path = Url.Route(Constants.RouteNames.Oidc.ClientPermissions, null);
            var url = new Uri(Request.RequestUri, path);
            message.ReturnUrl = url.AbsoluteUri;

            return new LoginResult(message, Request.GetOwinContext().Environment, options);
        }

        private async Task<IHttpActionResult> RenderPermissionsPage(string error = null)
        {
            var env = Request.GetOwinEnvironment();
            var clients = await this.clientPermissionsService.GetClientPermissionsAsync(User.GetSubjectId());
            var vm = new ClientPermissionsViewModel()
            {
                SiteName = options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                CurrentUser = User.GetName(),
                LogoutUrl = Url.Link(Constants.RouteNames.Logout, null),
                RevokePermissionUrl = Url.Link(Constants.RouteNames.Oidc.ClientPermissions, null),
                AntiForgery = AntiForgeryTokenValidator.GetAntiForgeryHiddenInput(env),
                Clients = clients,
                ErrorMessage = error
            };
            return new ClientPermissionsActionResult(this.viewSvc, Request.GetOwinEnvironment(), vm);
        }
    }
}
