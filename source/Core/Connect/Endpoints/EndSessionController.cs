/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Views;

namespace Thinktecture.IdentityServer.Core.Connect
{
  [HostAuthentication(Constants.PrimaryAuthenticationType)]
  [SecurityHeaders]
  [NoCache]
  public class EndSessionController : ApiController
  {
    private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

    private readonly IViewService _viewService;
    private readonly IdentityServerOptions _options;
    private readonly EndSessionResponseGenerator _responseGenerator;
    private readonly EndSessionRequestValidator _validator;

    public EndSessionController(
        IViewService viewService,
        EndSessionResponseGenerator responseGenerator,
        EndSessionRequestValidator validator,
        IdentityServerOptions options)
    {
      _viewService = viewService;
      _options = options;
      _responseGenerator = responseGenerator;
      _validator = validator;
    }

    [Route(Constants.RoutePaths.Oidc.EndSession, Name = Constants.RouteNames.Oidc.EndSession)]
    [HttpGet]
    public async Task<IHttpActionResult> Logout(HttpRequestMessage request)
    {
      Logger.Info("End session request");
      return await ProcessRequest(request.RequestUri.ParseQueryString());
    }

    [Route(Constants.RoutePaths.Oidc.EndSessionCallback, Name = Constants.RouteNames.Oidc.EndSessionCallback)]
    [HttpGet]
    public IHttpActionResult LogoutCallback()
    {
      Logger.Info("End session callback requested");

      return Ok();
    }

    async Task<IHttpActionResult> ProcessRequest(NameValueCollection parameters)
    {
      if (!_options.EndSessionEndpoint.IsEnabled)
      {
        Logger.Warn("Endpoint is disabled. Aborting");
        return NotFound();
      }

      var result = await _validator.ValidateProtocol(parameters);

      if (result.IsError)
      {
        return this.EndSessionError(
            result.ErrorType,
            result.Error);
      }

      var response = _responseGenerator.ProcessRequest(_validator.ValidatedRequest, User as ClaimsPrincipal);

      if (response.IsRedirect)
        return Redirect(response.RedirectUri);

      return RedirectToLogout(response.LogoutMessage, parameters);
    }

    IHttpActionResult RedirectToLogout(LogOutMessage message, NameValueCollection parameters)
    {
      message = message ?? new LogOutMessage();

      var path = Url.Route(Constants.RouteNames.Oidc.EndSession, null) + "?" + parameters.ToQueryString();
      var url = new Uri(Request.RequestUri, path);
      message.ReturnUrl = url.AbsoluteUri;

      return new LogoutResult(message, Request.GetOwinContext().Environment, _options.DataProtector);
    }

    IHttpActionResult EndSessionError(ErrorTypes errorTypes, string error)
    {
      var env = Request.GetOwinEnvironment();
      var errorModel = new ErrorViewModel
      {
        SiteName = _options.SiteName,
        SiteUrl = env.GetIdentityServerBaseUrl(),
        CurrentUser = User.GetName(),
        ErrorMessage = error
      };
      return new ErrorActionResult(_viewService, env, errorModel);
    }
  }
}