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

using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Events.Base;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.ViewModels;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ErrorPageFilter]
    [HostAuthentication(Constants.PRIMARY_AUTHENTICATION_TYPE)]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    internal class ClientPermissionsController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IClientPermissionsService _clientPermissionsService;
        private readonly IdentityServerOptions _options;
        private readonly IViewService _viewSvc;
        private readonly ILocalizationService _localizationService;
        private readonly IEventService _eventService;
        private readonly AntiForgeryToken _antiForgeryToken;

        public ClientPermissionsController(
            IClientPermissionsService clientPermissionsService, 
            IdentityServerOptions options, 
            IViewService viewSvc, 
            ILocalizationService localizationService,
            IEventService eventService,
            AntiForgeryToken antiForgeryToken)
        {
            _clientPermissionsService = clientPermissionsService;
            _options = options;
            _viewSvc = viewSvc;
            _localizationService = localizationService;
            _eventService = eventService;
            _antiForgeryToken = antiForgeryToken;
        }

        [Route(Constants.RoutePaths.CLIENT_PERMISSIONS)]
        [HttpGet]
        public async Task<IHttpActionResult> ShowPermissions()
        {
            Logger.Info("Permissions page requested");

            if (!_options.Endpoints.EnableClientPermissionsEndpoint)
            {
                Logger.Error("Permissions page disabled, returning 404");
                _eventService.RaiseFailureEndpointEvent(EventConstants.EndpointNames.CLIENT_PERMISSIONS, "endpoint disabled");
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

        [Route(Constants.RoutePaths.CLIENT_PERMISSIONS, Name = Constants.RouteNames.CLIENT_PERMISSIONS)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> RevokePermission(RevokeClientPermission model)
        {
            Logger.Info("Revoke permissions requested");
            
            if (!_options.Endpoints.EnableClientPermissionsEndpoint)
            {
                Logger.Error("Permissions page disabled, returning 404");
                _eventService.RaiseFailureEndpointEvent(EventConstants.EndpointNames.CLIENT_PERMISSIONS, "endpoint disabled");
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
                ModelState.AddModelError("ClientId", _localizationService.GetMessage(MessageIds.CLIENT_ID_REQUIRED));
            }

            if (model == null || ModelState.IsValid == false)
            {
                var error = ModelState.Where(x => x.Value.Errors.Any()).Select(x => x.Value.Errors.First().ErrorMessage).First();
                Logger.WarnFormat("Rendering error: {0}", error);
                return await RenderPermissionsPage(error);
            }

            Logger.InfoFormat("Revoking permissions for sub: {0}, name: {1}, clientID: {2}", User.GetSubjectId(), User.Identity.Name, model.ClientId);
            
            await _clientPermissionsService.RevokeClientPermissionsAsync(User.GetSubjectId(), model.ClientId);
            
            _eventService.RaiseClientPermissionsRevokedEvent(User as ClaimsPrincipal, model.ClientId);

            Logger.Info("Redirecting back to permissions page");

            return RedirectToRoute(Constants.RouteNames.CLIENT_PERMISSIONS, null);
        }

        private IHttpActionResult RedirectToLogin()
        {
            var message = new SignInMessage();

            var path = Url.Route(Constants.RouteNames.CLIENT_PERMISSIONS, null);
            var host = new Uri(Request.GetOwinEnvironment().GetIdentityServerHost());
            var url = new Uri(host, path);
            message.ReturnUrl = url.AbsoluteUri;
            return new LoginResult(Request.GetOwinContext().Environment, message);
        }

        private async Task<IHttpActionResult> RenderPermissionsPage(string error = null)
        {
            var env = Request.GetOwinEnvironment();
            var clients = await _clientPermissionsService.GetClientPermissionsAsync(User.GetSubjectId());
            var vm = new ClientPermissionsViewModel
            {
                RequestId = env.GetRequestId(),
                SiteName = _options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                CurrentUser = env.GetCurrentUserDisplayName(),
                LogoutUrl = env.GetIdentityServerLogoutUrl(),
                RevokePermissionUrl = Request.GetOwinContext().GetPermissionsPageUrl(),
                AntiForgery = _antiForgeryToken.GetAntiForgeryToken(),
                Clients = clients,
                ErrorMessage = error
            };
            return new ClientPermissionsActionResult(_viewSvc, Request.GetOwinEnvironment(), vm);
        }
    }
}
