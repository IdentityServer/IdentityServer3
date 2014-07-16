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
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFederation.Configuration;
using Thinktecture.IdentityServer.WsFederation.ResponseHandling;
using Thinktecture.IdentityServer.WsFederation.Results;
using Thinktecture.IdentityServer.WsFederation.Validation;
using System.Net.Http;

namespace Thinktecture.IdentityServer.WsFederation
{
    [HostAuthentication("idsrv")]
    [RoutePrefix("")]
    [NoCache]
    [SecurityHeaders(EnableCsp=false)]
    public class WsFederationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly CoreSettings _settings;
        private readonly SignInValidator _validator;
        private readonly SignInResponseGenerator _signInResponseGenerator;
        private readonly MetadataResponseGenerator _metadataResponseGenerator;
        private readonly ITrackingCookieService _cookies;
        private readonly InternalConfiguration _internalConfig;
        private readonly WsFederationSettings _wsFedSettings;
        private readonly WsFederationPluginOptions _wsFedOptions;

        public WsFederationController(CoreSettings settings, IUserService users, SignInValidator validator, SignInResponseGenerator signInResponseGenerator, MetadataResponseGenerator metadataResponseGenerator, ITrackingCookieService cookies, InternalConfiguration internalConfig, WsFederationSettings wsFedSettings, WsFederationPluginOptions wsFedOptions)
        {
            _settings = settings;
            _internalConfig = internalConfig;
            _wsFedSettings = wsFedSettings;
            _validator = validator;
            _signInResponseGenerator = signInResponseGenerator;
            _metadataResponseGenerator = metadataResponseGenerator;
            _cookies = cookies;
            _wsFedOptions = wsFedOptions;
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

                    var url = this.Request.GetOwinContext().Environment.GetIdentityServerLogoutUrl();
                    return Redirect(url);
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

            if (_wsFedSettings.MetadataEndpoint.IsEnabled == false)
            {
                Logger.Warn("Endpoint is disabled. Aborting.");
                return NotFound();
            }

            var ep = Request.GetOwinContext().Environment.GetIdentityServerBaseUrl() + this._wsFedOptions.MapPath;
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

            var url = LoginResult.GetRedirectUrl(message, this.Request.GetOwinContext().Environment, _internalConfig);
            return Redirect(url);
        }
    }
}