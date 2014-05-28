/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.IdentityModel.Services;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;
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
        private ILogger _logger;
  
        private SignInValidator _validator;
        private SignInResponseGenerator _signInResponseGenerator;
        private MetadataResponseGenerator _metadataResponseGenerator;
        private ICookieService _cookies;

        public WsFederationController(CoreSettings settings, IUserService users, ILogger logger, SignInValidator validator, SignInResponseGenerator signInResponseGenerator, MetadataResponseGenerator metadataResponseGenerator, ICookieService cookies)
        {
            _settings = settings;
            _logger = logger;

            _validator = validator;
            _signInResponseGenerator = signInResponseGenerator;
            _metadataResponseGenerator = metadataResponseGenerator;
            _cookies = cookies;
        }

        [Route("wsfed")]
        public async Task<IHttpActionResult> Get()
        {
            WSFederationMessage message;
            if (WSFederationMessage.TryCreateFromUri(Request.RequestUri, out message))
            {
                var signin = message as SignInRequestMessage;
                if (signin != null)
                {
                    return await ProcessSignInAsync(signin);
                }

                var signout = message as SignOutRequestMessage;
                if (signout != null)
                {
                    return RedirectToRoute(Constants.RouteNames.LogoutPrompt, null);
                }
            }

            return BadRequest("Invalid WS-Federation request");
        }

        [Route("wsfed/signout")]
        [HttpGet]
        public async Task<IHttpActionResult> SignOutCallback()
        {
            var urls = await _cookies.GetValuesAndDeleteCookieAsync();
            return new SignOutResult(urls);
        }

        [Route("wsfed/metadata")]
        public IHttpActionResult GetMetadata()
        {
            var ep = Request.GetBaseUrl(_settings.GetPublicHost()) + "wsfed";
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
            await _cookies.AddValueAsync(result.ReplyUrl);

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

            return new LoginResult(message, this.Request, settings);
        }
    }
}