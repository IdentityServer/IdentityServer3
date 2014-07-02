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
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFederation.Configuration;
using Thinktecture.IdentityServer.WsFederation.ResponseHandling;
using Thinktecture.IdentityServer.WsFederation.Results;
using Thinktecture.IdentityServer.WsFederation.Validation;

namespace Thinktecture.IdentityServer.WsFederation
{
    [HostAuthentication("idsrv")]
    [RoutePrefix("")]
    public class WsFederationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly CoreSettings _settings;
        private readonly WsFederationPluginOptions _wsfedOptions;
        private readonly SignInValidator _validator;
        private readonly SignInResponseGenerator _signInResponseGenerator;
        private readonly MetadataResponseGenerator _metadataResponseGenerator;
        private readonly ITrackingCookieService _cookies;
        private readonly InternalConfiguration _internalConfig;

        public WsFederationController(CoreSettings settings, IUserService users, SignInValidator validator, SignInResponseGenerator signInResponseGenerator, MetadataResponseGenerator metadataResponseGenerator, ITrackingCookieService cookies, InternalConfiguration internalConfig, WsFederationPluginOptions wsFedOptions)
        {
            _settings = settings;
            _internalConfig = internalConfig;
            _wsfedOptions = wsFedOptions;
            _validator = validator;
            _signInResponseGenerator = signInResponseGenerator;
            _metadataResponseGenerator = metadataResponseGenerator;
            _cookies = cookies;
        }

        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            Logger.Info("Start WS-Federation request");
            Logger.Debug(Request.RequestUri.AbsoluteUri);

            WSFederationMessage message;
            if (WSFederationMessage.TryCreateFromUri(Request.RequestUri, out message))
            {
                var signin = message as SignInRequestMessage;
                if (signin != null)
                {
                    Logger.Info("WsFederation signin request");
                    return await ProcessSignInAsync(signin);
                }

                var signout = message as SignOutRequestMessage;
                if (signout != null)
                {
                    Logger.Info("WsFederation signout request");

                    // todo
                    return Redirect(_wsfedOptions.LogoutPageUrl);
                }
            }

            return BadRequest("Invalid WS-Federation request");
        }

        [Route("signout")]
        [HttpGet]
        public async Task<IHttpActionResult> SignOutCallback()
        {
            Logger.Info("WS-Federation signout callback");

            var urls = await _cookies.GetValuesAndDeleteCookieAsync(WsFederationPluginOptions.CookieName);
            return new SignOutResult(urls);
        }

        [Route("metadata")]
        public IHttpActionResult GetMetadata()
        {
            Logger.Info("WS-Federation metadata request");

            if (_wsfedOptions.Factory.WsFederationSettings().MetadataEndpoint.Enabled == false)
            {
                Logger.Warn("Endpoint is disabled. Aborting.");
                return NotFound();
            }

            var ep = Request.GetBaseUrl(_settings.PublicHostName);
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

            var url = LoginResult.GetRedirectUrl(message, this.Request, settings, _internalConfig);
            return Redirect(url);
        }
    }
}