/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.IdentityModel.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFed.Configuration;
using Thinktecture.IdentityServer.WsFed.ResponseHandling;
using Thinktecture.IdentityServer.WsFed.Results;
using Thinktecture.IdentityServer.WsFed.Services;
using Thinktecture.IdentityServer.WsFed.Validation;

namespace Thinktecture.IdentityServer.WsFed
{
    [HostAuthentication("idsrv")]
    public class WsFederationController : ApiController
    {
        private readonly CoreSettings _settings;
        private readonly WsFederationPluginOptions _wsfedOptions;
        private readonly SignInValidator _validator;
        private readonly SignInResponseGenerator _signInResponseGenerator;
        private readonly MetadataResponseGenerator _metadataResponseGenerator;
        private readonly ITrackingCookieService _cookies;
        private readonly InternalConfiguration _internalConfig;

        private readonly ILog _logger;
  
        public WsFederationController(CoreSettings settings, IUserService users, SignInValidator validator, SignInResponseGenerator signInResponseGenerator, MetadataResponseGenerator metadataResponseGenerator, ITrackingCookieService cookies, InternalConfiguration internalConfig, WsFederationPluginOptions wsFedOptions)
        {
            _settings = settings;
            _internalConfig = internalConfig;
            _wsfedOptions = wsFedOptions;
            _validator = validator;
            _signInResponseGenerator = signInResponseGenerator;
            _metadataResponseGenerator = metadataResponseGenerator;
            _cookies = cookies;

            _logger = LogProvider.GetCurrentClassLogger();
        }

        [Route("wsfed")]
        public async Task<IHttpActionResult> Get()
        {
            _logger.Info("Start WS-Federation request");
            _logger.Debug(Request.RequestUri.AbsoluteUri);

            WSFederationMessage message;
            if (WSFederationMessage.TryCreateFromUri(Request.RequestUri, out message))
            {
                var signin = message as SignInRequestMessage;
                if (signin != null)
                {
                    _logger.Info("WsFederation signin request");
                    return await ProcessSignInAsync(signin);
                }

                var signout = message as SignOutRequestMessage;
                if (signout != null)
                {
                    _logger.Info("WsFederation signout request");
                    return RedirectToRoute(Constants.RouteNames.LogoutPrompt, null);
                }
            }

            return BadRequest("Invalid WS-Federation request");
        }

        [Route("wsfed/signout")]
        [HttpGet]
        public async Task<IHttpActionResult> SignOutCallback()
        {
            _logger.Info("WS-Federation signout callback");

            var urls = await _cookies.GetValuesAndDeleteCookieAsync(WsFederationPluginOptions.CookieName);
            return new SignOutResult(urls);
        }

        [Route("wsfed/metadata")]
        public IHttpActionResult GetMetadata()
        {
            _logger.Info("WS-Federation metadata request");

            if (_wsfedOptions.EnableFederationMetadata == false)
            {
                _logger.Warn("Endpoint is disabled. Aborting.");
                return NotFound();
            }

            var ep = Request.GetBaseUrl(_settings.PublicHostName) + "wsfed";
            var entity = _metadataResponseGenerator.Generate(ep);

            return new MetadataResult(entity);
        }

        private async Task<IHttpActionResult> ProcessSignInAsync(SignInRequestMessage msg)
        {
            var result = await _validator.ValidateAsync(msg, User as ClaimsPrincipal);

            if (result.IsSignInRequired)
            {
                return RedirectToLogin(_settings, result);
            }
            if (result.IsError)
            {
                return BadRequest(result.Error);
            }

            var responseMessage = await _signInResponseGenerator.GenerateResponseAsync(result);
            await _cookies.AddValueAsync(WsFederationPluginOptions.CookieName, result.ReplyUrl);

            return new SignInResult(responseMessage);
        }

        IHttpActionResult RedirectToLogin(CoreSettings settings, SignInValidationResult result)
        {
            var message = new SignInMessage();
            message.ReturnUrl = Request.RequestUri.AbsoluteUri;

            if (result.HomeRealm.IsPresent())
            {
                message.IdP = result.HomeRealm;
            }

            return new LoginResult(message, this.Request, settings, _internalConfig);
        }
    }
}