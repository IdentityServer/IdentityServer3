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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Configuration.Hosting;
using IdentityServer3.Core.Events;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Resources;
using IdentityServer3.Core.Results;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.ViewModels;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Endpoints
{
    [ErrorPageFilter]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    internal class ClientPermissionsController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IClientPermissionsService clientPermissionsService;
        private readonly IdentityServerOptions options;
        private readonly IViewService viewSvc;
        private readonly ILocalizationService localizationService;
        private readonly IEventService eventService;
        private readonly AntiForgeryToken antiForgeryToken;

        public ClientPermissionsController(
            IClientPermissionsService clientPermissionsService, 
            IdentityServerOptions options, 
            IViewService viewSvc, 
            ILocalizationService localizationService,
            IEventService eventService,
            AntiForgeryToken antiForgeryToken)
        {
            this.clientPermissionsService = clientPermissionsService;
            this.options = options;
            this.viewSvc = viewSvc;
            this.localizationService = localizationService;
            this.eventService = eventService;
            this.antiForgeryToken = antiForgeryToken;
        }

        [Route(Constants.RoutePaths.ClientPermissions)]
        [HttpGet]
        public async Task<IHttpActionResult> ShowPermissions()
        {
            Logger.Info("Permissions page requested");

            if (!options.Endpoints.EnableClientPermissionsEndpoint)
            {
                Logger.Error("Permissions page disabled, returning 404");
                await eventService.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.ClientPermissions, "endpoint disabled");
                return NotFound();
            }

            if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
            {
                Logger.Info("User not authenticated, redirecting to login");
                return RedirectToLogin();
            }

            Logger.Info("Rendering permissions page");

            return await RenderPermissionsPage();
        }

        [Route(Constants.RoutePaths.ClientPermissions, Name = Constants.RouteNames.ClientPermissions)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> RevokePermission(RevokeClientPermission model)
        {
            Logger.Info("Revoke permissions requested");
            
            if (!options.Endpoints.EnableClientPermissionsEndpoint)
            {
                Logger.Error("Permissions page disabled, returning 404");
                await eventService.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.ClientPermissions, "endpoint disabled");
                return NotFound();
            }
            
            if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
            {
                Logger.Info("User not authenticated, redirecting to login");
                return RedirectToLogin();
            }

            if (model != null && String.IsNullOrWhiteSpace(model.ClientId))
            {
                Logger.Warn("No model or client id submitted");
                ModelState.AddModelError("ClientId", localizationService.GetMessage(MessageIds.ClientIdRequired));
            }

            if (model == null || ModelState.IsValid == false)
            {
                var error = ModelState.Where(x => x.Value.Errors.Any()).Select(x => x.Value.Errors.First().ErrorMessage).First();
                Logger.WarnFormat("Rendering error: {0}", error);
                return await RenderPermissionsPage(error);
            }

            Logger.InfoFormat("Revoking permissions for sub: {0}, name: {1}, clientID: {2}", User.GetSubjectId(), User.Identity.Name, model.ClientId);
            
            await this.clientPermissionsService.RevokeClientPermissionsAsync(User.GetSubjectId(), model.ClientId);

            await eventService.RaiseClientPermissionsRevokedEventAsync(User as ClaimsPrincipal, model.ClientId);

            Logger.Info("Redirecting back to permissions page");

            return RedirectToRoute(Constants.RouteNames.ClientPermissions, null);
        }

        private IHttpActionResult RedirectToLogin()
        {
            var message = new SignInMessage();

            var path = Url.Route(Constants.RouteNames.ClientPermissions, null);
            var host = new Uri(Request.GetOwinEnvironment().GetIdentityServerHost());
            var url = new Uri(host, path);
            message.ReturnUrl = url.AbsoluteUri;
            return new LoginResult(Request.GetOwinContext().Environment, message);
        }

        private async Task<IHttpActionResult> RenderPermissionsPage(string error = null)
        {
            var env = Request.GetOwinEnvironment();
            var clients = await this.clientPermissionsService.GetClientPermissionsAsync(User.GetSubjectId());
            var vm = new ClientPermissionsViewModel
            {
                RequestId = env.GetRequestId(),
                SiteName = options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                CurrentUser = env.GetCurrentUserDisplayName(),
                LogoutUrl = env.GetIdentityServerLogoutUrl(),
                RevokePermissionUrl = Request.GetOwinContext().GetPermissionsPageUrl(),
                AntiForgery = antiForgeryToken.GetAntiForgeryToken(),
                Clients = clients,
                ErrorMessage = error
            };
            return new ClientPermissionsActionResult(this.viewSvc, Request.GetOwinEnvironment(), vm);
        }
    }
}
