/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    [ErrorPageFilter]
    public class AuthenticationController : ApiController
    {
        ILogger logger;
        IUserService userService;
        CoreSettings settings;
        AuthenticationOptions authenticationOptions;
        IExternalClaimsFilter externalClaimsFilter;
        InternalConfiguration internalConfiguration;

        public AuthenticationController(ILogger logger, IUserService userService, CoreSettings settings, IExternalClaimsFilter externalClaimsFilter, AuthenticationOptions authenticationOptions, InternalConfiguration internalConfiguration)
        {
            this.logger = logger;
            this.userService = userService;
            this.settings = settings;
            this.externalClaimsFilter = externalClaimsFilter;
            this.authenticationOptions = authenticationOptions;
            this.internalConfiguration = internalConfiguration;
        }

        [Route(Constants.RoutePaths.Login, Name = Constants.RouteNames.Login)]
        [HttpGet]
        public IHttpActionResult Login([FromUri] string message = null)
        {
            logger.Start("[AuthenticationController.Login] called");

            if (message != null)
            {
                logger.Verbose("[AuthenticationController.LoginLocal] non-null message");
                
                var signIn = SaveLoginRequestMessage(message);
                if (signIn.IdP.IsPresent())
                {
                    return Redirect(Url.Link(Constants.RouteNames.LoginExternal, new { provider=signIn.IdP }));
                }
            }
            else
            {
                logger.Verbose("[AuthenticationController.LoginLocal] null message");
                VerifyLoginRequestMessage();
            }

            return RenderLoginPage();
        }

        [Route(Constants.RoutePaths.Login)]
        [HttpPost]
        public async Task<IHttpActionResult> LoginLocal(LoginCredentials model)
        {
            logger.Start("[AuthenticationController.LoginLocal] called");

            if (model == null)
            {
                logger.Verbose("[AuthenticationController.LoginLocal] no model");
                return RenderLoginPage(Messages.InvalidUsernameOrPassword);
            }

            if (!ModelState.IsValid)
            {
                logger.Verbose("[AuthenticationController.LoginLocal] model not valid");
                return RenderLoginPage(ModelState.GetError(), model.Username);
            }

            var authResult = await userService.AuthenticateLocalAsync(model.Username, model.Password);
            if (authResult == null)
            {
                logger.Verbose("[AuthenticationController.LoginLocal] authenticate returned null");
                return RenderLoginPage(Messages.InvalidUsernameOrPassword, model.Username);
            }

            if (authResult.IsError)
            {
                logger.Verbose("[AuthenticationController.LoginLocal] authenticate returned an error message");
                return RenderLoginPage(authResult.ErrorMessage, model.Username);
            }

            return SignInAndRedirect(
                 authResult,
                 Constants.AuthenticationMethods.Password,
                 Constants.BuiltInIdentityProvider);
        }

        [Route(Constants.RoutePaths.LoginExternal, Name = Constants.RouteNames.LoginExternal)]
        [HttpGet]
        public IHttpActionResult LoginExternal(string provider)
        {
            logger.Start("[AuthenticationController.LoginExternal] called");

            VerifyLoginRequestMessage();

            var ctx = Request.GetOwinContext();
            var authProp = new Microsoft.Owin.Security.AuthenticationProperties
            {
                RedirectUri = Url.Route(Constants.RouteNames.LoginExternalCallback, null)
            };
            Request.GetOwinContext().Authentication.Challenge(authProp, provider);
            return Unauthorized();
        }

        [Route(Constants.RoutePaths.LoginExternalCallback, Name = Constants.RouteNames.LoginExternalCallback)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginExternalCallback()
        {
            logger.Start("[AuthenticationController.LoginExternalCallback] called");

            var ctx = Request.GetOwinContext();

            string currentSubject = null;
            var currentAuth = await ctx.Authentication.AuthenticateAsync(Constants.PrimaryAuthenticationType);
            if (currentAuth != null &&
                currentAuth.Identity != null &&
                currentAuth.Identity.IsAuthenticated)
            {
                currentSubject = currentAuth.Identity.Claims.GetValue(Constants.ClaimTypes.Subject);
            }

            logger.VerboseFormat("[AuthenticationController.LoginExternalCallback] current subject: {0}", currentSubject ?? "-anonymous-");

            var externalAuthResult = await ctx.Authentication.AuthenticateAsync(Constants.ExternalAuthenticationType);
            if (externalAuthResult == null ||
                externalAuthResult.Identity == null ||
                !externalAuthResult.Identity.Claims.Any())
            {
                logger.Verbose("[AuthenticationController.LoginExternalCallback] no external identity -- exiting to login page");
                return RedirectToRoute(Constants.RouteNames.Login, null);
            }

            var claims = externalAuthResult.Identity.Claims;
            var externalIdentity = GetExternalIdentity(claims);
            if (externalIdentity == null)
            {
                logger.Verbose("[AuthenticationController.LoginExternalCallback] null external identity");
                return RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            var authResult = await userService.AuthenticateExternalAsync(currentSubject, externalIdentity);
            if (authResult == null)
            {
                logger.Verbose("[AuthenticationController.LoginExternalCallback] authenticate external returned null");
                return RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            if (authResult.IsError)
            {
                logger.Verbose("[AuthenticationController.LoginExternalCallback] authenticate external returned error message");
                return RenderLoginPage(authResult.ErrorMessage);
            }

            return SignInAndRedirect(
                authResult,
                Constants.AuthenticationMethods.External,
                authResult.Provider);
        }

        ExternalIdentity GetExternalIdentity(IEnumerable<Claim> claims)
        {
            if (claims == null || !claims.Any())
            {
                return null;
            }

            var subClaim = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Subject);
            if (subClaim == null)
            {
                subClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (subClaim == null)
                {
                    return null;
                }
            }

            claims = claims.Except(new Claim[] { subClaim });

            var idp = new IdentityProvider { Name = subClaim.Issuer };
            if (this.externalClaimsFilter != null)
            {
                claims = externalClaimsFilter.Filter(idp, claims);
            }

            claims = claims ?? Enumerable.Empty<Claim>();

            return new ExternalIdentity
            {
                Provider = idp,
                ProviderId = subClaim.Value,
                Claims = claims
            };
        }

        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.Logout)]
        [HttpGet, HttpPost]
        public IHttpActionResult Logout()
        {
            logger.Start("[AuthenticationController.Logout] called");

            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);

            ClearLoginRequestMessage();

            var baseUrl = Request.GetBaseUrl(settings.GetPublicHost());
            var urls = new List<string>();
            foreach(var url in this.internalConfiguration.PluginDependencies.SignOutCallbackUrls)
            {
                var tmp = url;
                if (tmp.StartsWith("/")) tmp = tmp.Substring(1);
                urls.Add(baseUrl + tmp);
            }

            return new EmbeddedHtmlResult(Request,
                   new LayoutModel
                   {
                       Server = settings.GetSiteName(),
                       Page = "logout",
                       PageModel = new
                       {
                           signOutUrls = urls.ToArray()
                       }
                   });
        }

        [Route(Constants.RoutePaths.ResumeLoginFromRedirect, Name = Constants.RouteNames.ResumeLoginFromRedirect)]
        [HttpGet]
        public async Task<IHttpActionResult> ResumeLoginFromRedirect()
        {
            logger.Start("[AuthenticationController.ResumeLoginFromRedirect] called");

            var ctx = Request.GetOwinContext();
            var redirectAuthResult = await ctx.Authentication.AuthenticateAsync(Constants.PartialSignInAuthenticationType);
            if (redirectAuthResult == null ||
                redirectAuthResult.Identity == null)
            {
                logger.Verbose("[AuthenticationController.ResumeLoginFromRedirect] no redirect identity - exiting to login page");
                return RedirectToRoute(Constants.RouteNames.Login, null);
            }

            var subject = redirectAuthResult.Identity.GetSubjectId();
            var name = redirectAuthResult.Identity.GetName();

            var result = new Thinktecture.IdentityServer.Core.Services.AuthenticateResult(subject, name);
            var method = redirectAuthResult.Identity.GetAuthenticationMethod();
            var idp = redirectAuthResult.Identity.GetIdentityProvider();
            var authTime = redirectAuthResult.Identity.GetAuthenticationTimeEpoch();

            return SignInAndRedirect(result, method, idp, authTime);
        }

        private IHttpActionResult SignInAndRedirect(
            Thinktecture.IdentityServer.Core.Services.AuthenticateResult authResult,
            string authenticationMethod,
            string identityProvider,
            long authTime = 0)
        {
            logger.Verbose("[AuthenticationController.SignInAndRedirect] called");

            if (authResult == null) throw new ArgumentNullException("authResult");
            if (String.IsNullOrWhiteSpace(authenticationMethod)) throw new ArgumentNullException("authenticationMethod");
            if (String.IsNullOrWhiteSpace(identityProvider)) throw new ArgumentNullException("identityProvider");

            var signInMessage = LoadLoginRequestMessage();

            var issuer = authResult.IsPartialSignIn ?
                Constants.PartialSignInAuthenticationType :
                Constants.PrimaryAuthenticationType;

            var principal = IdentityServerPrincipal.Create(
                authResult.Subject,
                authResult.Name,
                authenticationMethod,
                identityProvider,
                issuer,
                authTime);

            var id = principal.Identities.First();

            if (authResult.IsPartialSignIn)
            {
                // TODO: put original return URL into cookie with a GUID ID
                // and put the ID as route param for the resume URL. then
                // we can always call ClearLoginRequestMessage()
                id.AddClaim(new Claim(Constants.ClaimTypes.PartialLoginReturnUrl, Url.Route(Constants.RouteNames.ResumeLoginFromRedirect, null)));

                // allow redircting code to add claims for target page
                id.AddClaims(authResult.RedirectClaims);
            }

            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);
            ctx.Authentication.SignIn(id);

            if (authResult.IsPartialSignIn)
            {
                logger.Verbose("[AuthenticationController.SignInAndRedirect] partial login requested, redirecting to requested url");

                var uri = new Uri(ctx.Request.Uri, authResult.PartialSignInRedirectPath.Value);
                return Redirect(uri);
            }
            else
            {
                logger.Verbose("[AuthenticationController.SignInAndRedirect] normal login requested, redirecting back to authorization");

                // TODO -- manage this state better if we're doing redirect to custom page
                // would rather the redirect URL from request message put into cookie
                // and named with a nonce, then the resume url + nonce set as claim
                // in principal above so page being redirected to can know what url to return to
                ClearLoginRequestMessage();

                return Redirect(signInMessage.ReturnUrl);
            }
        }

        private IHttpActionResult RenderLoginPage(string errorMessage = null, string username = null)
        {
            logger.Verbose("[AuthenticationController.RenderLoginPage] called");

            var ctx = Request.GetOwinContext();
            var providers =
                from p in ctx.Authentication.GetAuthenticationTypes(d => d.Caption.IsPresent())
                select new { name = p.Caption, url = Url.Route(Constants.RouteNames.LoginExternal, new { provider = p.AuthenticationType }) };

            var links = this.authenticationOptions.LoginPageLinks;

            return new EmbeddedHtmlResult(
                Request,
                new LayoutModel
                {
                    Server = settings.GetSiteName(),
                    Page = "login",
                    ErrorMessage = errorMessage,
                    PageModel = new
                    {
                        url = Url.Route(Constants.RouteNames.Login, null),
                        username = username,
                        providers = providers.ToArray(),
                        links = links
                    }
                });
        }

        const string LoginRequestMessageCookieName = "idsrv.login.message";
        private void ClearLoginRequestMessage()
        {
            logger.Verbose("[AuthenticationController.ClearLoginRequestMessage] called");

            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                LoginRequestMessageCookieName,
                ".",
                new Microsoft.Owin.CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(-1),
                    HttpOnly = true,
                    Secure = Request.RequestUri.Scheme == Uri.UriSchemeHttps
                });
        }

        private SignInMessage SaveLoginRequestMessage(string message)
        {
            logger.Verbose("[AuthenticationController.SaveLoginRequestMessage] called");

            var protection = settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);

            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                LoginRequestMessageCookieName,
                message,
                new Microsoft.Owin.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.RequestUri.Scheme == Uri.UriSchemeHttps
                });

            return signInMessage;
        }

        private SignInMessage LoadLoginRequestMessage()
        {
            logger.Verbose("[AuthenticationController.LoadLoginRequestMessage] called");

            var ctx = Request.GetOwinContext();
            var message = ctx.Request.Cookies[LoginRequestMessageCookieName];

            if (message.IsMissing())
            {
                logger.Error("LoginRequestMessage cookie is empty.");
                throw new Exception("LoginRequestMessage cookie is empty.");
            }

            var protection = settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);

            return signInMessage;
        }

        private void VerifyLoginRequestMessage()
        {
            logger.Verbose("[AuthenticationController.VerifyLoginRequestMessage] called");

            var ctx = Request.GetOwinContext();
            var message = ctx.Request.Cookies[LoginRequestMessageCookieName];

            var protection = settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);
        }
    }
}