/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.IdentityModel.Services;
using System.Security.Claims;
using System.Web.Http;
using System.Net.Http;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFed.ResponseHandling;
using Thinktecture.IdentityServer.WsFed.Results;
using Thinktecture.IdentityServer.WsFed.Validation;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.WsFed.Services;
using System.Threading.Tasks;
using System;

namespace Thinktecture.IdentityServer.WsFed
{
    [HostAuthentication("idsrv")]
    public class WsFederationController : ApiController
    {
        private readonly ICoreSettings _settings;
        private readonly IUserService _users;

        private ILogger _logger;
        private SignInValidator _validator;
        private SignInResponseGenerator _signInResponseGenerator;
        private MetadataResponseGenerator _metadataResponseGenerator;


        public WsFederationController(ICoreSettings settings, IUserService users, ILogger logger)
        {
            _settings = settings;
            _users = users;
            _logger = logger;

            _validator = new SignInValidator(logger);
            _signInResponseGenerator = new SignInResponseGenerator(logger, settings);
            _metadataResponseGenerator = new MetadataResponseGenerator(logger, settings);
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
                    return await ProcessSignOutAsync(signout);
                }
            }

            return BadRequest("Invalid WS-Federation request");
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
            var result = _validator.Validate(msg, User as ClaimsPrincipal);

            if (result.IsSignInRequired)
            {
                return RedirectToLogin(_settings);
            }
            if (result.IsError)
            {
                return BadRequest(result.Error);
            }

            var response = _signInResponseGenerator.GenerateResponse(result);

            var cookies = new CookieMiddlewareCookieService(Request.GetOwinContext());
            await cookies.AddValueAsync(result.ReplyUrl);

            return new WsFederationResult(response.SignInResponseMessage);
        }

        private Task<IHttpActionResult> ProcessSignOutAsync(SignOutRequestMessage msg)
        {
            throw new NotImplementedException();
        }

        IHttpActionResult RedirectToLogin(ICoreSettings settings)
        {
            var message = new SignInMessage();
            message.ReturnUrl = Request.RequestUri.AbsoluteUri;

            return new LoginResult(message, this.Request, settings);
        }
    }
}