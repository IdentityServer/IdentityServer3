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

using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
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
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    public class AuthenticationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IOwinContext context;
        private readonly IViewService viewService;
        private readonly IUserService userService;
        private readonly IdentityServerOptions options;
        private readonly IClientStore clientStore;
        private readonly IEventService eventService;
        private readonly ILocalizationService localizationService;
        private readonly SessionCookie sessionCookie;
        private readonly MessageCookie<SignInMessage> signInMessageCookie;
        private readonly MessageCookie<SignOutMessage> signOutMessageCookie;
        private readonly LastUserNameCookie lastUsernameCookie;
        private readonly AntiForgeryToken antiForgeryToken;

        public AuthenticationController(
            OwinEnvironmentService owin,
            IViewService viewService, 
            IUserService userService, 
            IdentityServerOptions idSvrOptions, 
            IClientStore clientStore, 
            IEventService eventService,
            ILocalizationService localizationService,
            SessionCookie sessionCookie, 
            MessageCookie<SignInMessage> signInMessageCookie,
            MessageCookie<SignOutMessage> signOutMessageCookie,
            LastUserNameCookie lastUsernameCookie,
            AntiForgeryToken antiForgeryToken)
        {
            this.context = new OwinContext(owin.Environment);
            this.viewService = viewService;
            this.userService = userService;
            this.options = idSvrOptions;
            this.clientStore = clientStore;
            this.eventService = eventService;
            this.localizationService = localizationService;
            this.sessionCookie = sessionCookie;
            this.signInMessageCookie = signInMessageCookie;
            this.signOutMessageCookie = signOutMessageCookie;
            this.lastUsernameCookie = lastUsernameCookie;
            this.antiForgeryToken = antiForgeryToken;
        }

        [Route(Constants.RoutePaths.Login, Name = Constants.RouteNames.Login)]
        [HttpGet]
        public async Task<IHttpActionResult> Login(string signin)
        {
            Logger.Info("Login page requested");

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            var signInMessage = signInMessageCookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            Logger.DebugFormat("signin message passed to login: {0}", JsonConvert.SerializeObject(signInMessage, Formatting.Indented));

            var authResult = await userService.PreAuthenticateAsync(signInMessage);
            if (authResult != null)
            {
                if (authResult.IsError)
                {
                    Logger.WarnFormat("user service returned an error message: {0}", authResult.ErrorMessage);
                    
                    eventService.RaisePreLoginFailureEvent(signin, signInMessage, authResult.ErrorMessage);
                    
                    return RenderErrorPage(authResult.ErrorMessage);
                }

                Logger.Info("user service returned a login result");

                eventService.RaisePreLoginSuccessEvent(signin, signInMessage, authResult);
                
                return SignInAndRedirect(signInMessage, signin, authResult);
            }

            if (signInMessage.IdP.IsPresent())
            {
                Logger.InfoFormat("identity provider requested, redirecting to: {0}", signInMessage.IdP);
                return Redirect(context.GetExternalProviderLoginUrl(signInMessage.IdP, signin));
            }

            return await RenderLoginPage(signInMessage, signin);
        }

        [Route(Constants.RoutePaths.Login)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> LoginLocal(string signin, LoginCredentials model)
        {
            Logger.Info("Login page submitted");

            if (this.options.AuthenticationOptions.EnableLocalLogin == false)
            {
                Logger.Warn("EnableLocalLogin disabled -- returning 405 MethodNotAllowed");
                return StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            var signInMessage = signInMessageCookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            if (model == null)
            {
                Logger.Error("no data submitted");
                return await RenderLoginPage(signInMessage, signin, localizationService.GetMessage(MessageIds.InvalidUsernameOrPassword));
            }

            if (String.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", localizationService.GetMessage(MessageIds.UsernameRequired));
            }
            
            if (String.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", localizationService.GetMessage(MessageIds.PasswordRequired));
            }

            model.RememberMe = options.AuthenticationOptions.CookieOptions.CalculateRememberMeFromUserInput(model.RememberMe);

            if (!ModelState.IsValid)
            {
                Logger.Warn("validation error: username or password missing");
                return await RenderLoginPage(signInMessage, signin, ModelState.GetError(), model.Username, model.RememberMe == true);
            }

            var authResult = await userService.AuthenticateLocalAsync(model.Username, model.Password, signInMessage);
            if (authResult == null)
            {
                Logger.WarnFormat("user service indicated incorrect username or password for username: {0}", model.Username);
                
                var errorMessage = localizationService.GetMessage(MessageIds.InvalidUsernameOrPassword);
                eventService.RaiseLocalLoginFailureEvent(model.Username, signin, signInMessage, errorMessage);
                
                return await RenderLoginPage(signInMessage, signin, errorMessage, model.Username, model.RememberMe == true);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned an error message: {0}", authResult.ErrorMessage);

                eventService.RaiseLocalLoginFailureEvent(model.Username, signin, signInMessage, authResult.ErrorMessage);
                
                return await RenderLoginPage(signInMessage, signin, authResult.ErrorMessage, model.Username, model.RememberMe == true);
            }

            eventService.RaiseLocalLoginSuccessEvent(model.Username, signin, signInMessage, authResult);

            lastUsernameCookie.SetValue(model.Username);

            return SignInAndRedirect(signInMessage, signin, authResult, model.RememberMe);
        }

        [Route(Constants.RoutePaths.LoginExternal, Name = Constants.RouteNames.LoginExternal)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginExternal(string signin, string provider)
        {
            Logger.InfoFormat("External login requested for provider: {0}", provider);

            if (provider.IsMissing())
            {
                Logger.Error("No provider passed");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoExternalProvider));
            }

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            var signInMessage = signInMessageCookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            if (!(await clientStore.IsValidIdentityProviderAsync(signInMessage.ClientId, provider)))
            {
                Logger.ErrorFormat("Provider {0} not allowed for client: {1}", provider, signInMessage.ClientId);
                return RenderErrorPage();
            }

            var authProp = new Microsoft.Owin.Security.AuthenticationProperties
            {
                RedirectUri = Url.Route(Constants.RouteNames.LoginExternalCallback, null)
            };

            // add the id to the dictionary so we can recall the cookie id on the callback
            authProp.Dictionary.Add(Constants.Authentication.SigninId, signin);
            authProp.Dictionary.Add(Constants.Authentication.KatanaAuthenticationType, provider);
            context.Authentication.Challenge(authProp, provider);
            
            return Unauthorized();
        }

        [Route(Constants.RoutePaths.LoginExternalCallback, Name = Constants.RouteNames.LoginExternalCallback)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginExternalCallback(string error = null)
        {
            Logger.Info("Callback invoked from external identity provider");
            
            if (error.IsPresent())
            {
                Logger.ErrorFormat("External identity provider returned error: {0}", error);
                eventService.RaiseExternalLoginErrorEvent(error);
                return RenderErrorPage(String.Format(localizationService.GetMessage(MessageIds.ExternalProviderError), error));
            }

            var signInId = await context.GetSignInIdFromExternalProvider();
            if (signInId.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            var signInMessage = signInMessageCookie.Read(signInId);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            var user = await context.GetIdentityFromExternalProvider();
            if (user == null)
            {
                Logger.Error("no identity from external identity provider");
                return await RenderLoginPage(signInMessage, signInId, localizationService.GetMessage(MessageIds.NoMatchingExternalAccount));
            }

            var externalIdentity = ExternalIdentity.FromClaims(user.Claims);
            if (externalIdentity == null)
            {
                Logger.Error("no subject or unique identifier claims from external identity provider");
                return await RenderLoginPage(signInMessage, signInId, localizationService.GetMessage(MessageIds.NoMatchingExternalAccount));
            }

            Logger.InfoFormat("external user provider: {0}, provider ID: {1}", externalIdentity.Provider, externalIdentity.ProviderId);

            var authResult = await userService.AuthenticateExternalAsync(externalIdentity, signInMessage);
            if (authResult == null)
            {
                Logger.Warn("user service failed to authenticate external identity");
                
                var msg = localizationService.GetMessage(MessageIds.NoMatchingExternalAccount);
                eventService.RaiseExternalLoginFailureEvent(externalIdentity, signInId, signInMessage, msg);
                
                return await RenderLoginPage(signInMessage, signInId, msg);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned error message: {0}", authResult.ErrorMessage);

                eventService.RaiseExternalLoginFailureEvent(externalIdentity, signInId, signInMessage, authResult.ErrorMessage);
                
                return await RenderLoginPage(signInMessage, signInId, authResult.ErrorMessage);
            }

            eventService.RaiseExternalLoginSuccessEvent(externalIdentity, signInId, signInMessage, authResult);

            return SignInAndRedirect(signInMessage, signInId, authResult);
        }

        [Route(Constants.RoutePaths.ResumeLoginFromRedirect, Name = Constants.RouteNames.ResumeLoginFromRedirect)]
        [HttpGet]
        public async Task<IHttpActionResult> ResumeLoginFromRedirect(string resume)
        {
            Logger.Info("Callback requested to resume login from partial login");

            if (resume.IsMissing())
            {
                Logger.Error("no resumeId passed");
                return RenderErrorPage();
            }

            var user = await context.GetIdentityFromPartialSignIn();
            if (user == null)
            {
                Logger.Error("no identity from partial login");
                return RenderErrorPage();
            }

            var type = GetClaimTypeForResumeId(resume);
            var resumeClaim = user.FindFirst(type);
            if (resumeClaim == null)
            {
                Logger.Error("no claim matching resumeId");
                return RenderErrorPage();
            }

            var signInId = resumeClaim.Value;
            if (signInId.IsMissing())
            {
                Logger.Error("No signin id found in resume claim");
                return RenderErrorPage();
            }

            var signInMessage = signInMessageCookie.Read(signInId);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(localizationService.GetMessage(MessageIds.NoSignInCookie));
            }

            // check to see if the partial login has all the claim types needed to login
            AuthenticateResult result = null;
            if (Constants.AuthenticateResultClaimTypes.All(claimType => user.HasClaim(claimType)))
            {
                Logger.Info("Authentication claims found -- logging user in");
                
                // the user/subject was known, so pass thru (without the redirect claims)
                if (user.HasClaim(Constants.ClaimTypes.PartialLoginReturnUrl))
                {
                    user.RemoveClaim(user.FindFirst(Constants.ClaimTypes.PartialLoginReturnUrl));
                }
                if (user.HasClaim(Constants.ClaimTypes.ExternalProviderUserId))
                {
                    user.RemoveClaim(user.FindFirst(Constants.ClaimTypes.ExternalProviderUserId));
                }
                if (user.HasClaim(GetClaimTypeForResumeId(resume)))
                {
                    user.RemoveClaim(user.FindFirst(GetClaimTypeForResumeId(resume)));
                }
                
                result = new AuthenticateResult(new ClaimsPrincipal(user));

                eventService.RaisePartialLoginCompleteEvent(user, signInId, signInMessage);
            }
            else
            {
                Logger.Info("Authentication claims not found -- looking for ExternalProviderUserId to call AuthenticateExternalAsync");
                
                // the user was not known, we need to re-execute AuthenticateExternalAsync
                // to obtain a subject to proceed
                var externalProviderClaim = user.FindFirst(Constants.ClaimTypes.ExternalProviderUserId);
                if (externalProviderClaim == null)
                {
                    Logger.Error("No ExternalProviderUserId claim found -- rendering error page");
                    return RenderErrorPage();
                }

                var provider = externalProviderClaim.Issuer;
                var providerId = externalProviderClaim.Value;
                var externalId = new ExternalIdentity
                {
                    Provider = provider,
                    ProviderId = providerId,
                    Claims = user.Claims
                };

                result = await userService.AuthenticateExternalAsync(externalId, signInMessage);

                if (result == null)
                {
                    Logger.Warn("user service failed to authenticate external identity");
                    
                    var msg = localizationService.GetMessage(MessageIds.NoMatchingExternalAccount);
                    eventService.RaiseExternalLoginFailureEvent(externalId, signInId, signInMessage, msg);
                    
                    return await RenderLoginPage(signInMessage, signInId, msg);
                }

                if (result.IsError)
                {
                    Logger.WarnFormat("user service returned error message: {0}", result.ErrorMessage);

                    eventService.RaiseExternalLoginFailureEvent(externalId, signInId, signInMessage, result.ErrorMessage);
                    
                    return await RenderLoginPage(signInMessage, signInId, result.ErrorMessage);
                }

                eventService.RaiseExternalLoginSuccessEvent(externalId, signInId, signInMessage, result);
            }

            return SignInAndRedirect(signInMessage, signInId, result);
        }

        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.LogoutPrompt)]
        [HttpGet]
        public async Task<IHttpActionResult> LogoutPrompt(string id = null)
        {
            var user = (ClaimsPrincipal)User;
            if (user == null || user.Identity.IsAuthenticated == false)
            {
                // user is already logged out, so just trigger logout cleanup
                return await Logout(id);
            }

            var sub = user.GetSubjectId();
            Logger.InfoFormat("Logout prompt for subject: {0}", sub);

            var message = signOutMessageCookie.Read(id);
            if (message != null && message.ClientId.IsPresent())
            {
                Logger.InfoFormat("SignOutMessage present (from client {0}), performing logout", message.ClientId);
                return await Logout(id);
            }

            if (!this.options.AuthenticationOptions.EnableSignOutPrompt)
            {
                Logger.InfoFormat("EnableSignOutPrompt set to false, performing logout");
                return await Logout(id);
            }

            Logger.InfoFormat("EnableSignOutPrompt set to true, rendering logout prompt");
            return await RenderLogoutPromptPage(id);
        }

        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.Logout)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> Logout(string id = null)
        {
            var user = (ClaimsPrincipal)User;
            if (user != null && user.Identity.IsAuthenticated)
            {
                var sub = user.GetSubjectId();
                Logger.InfoFormat("Logout requested for subject: {0}", sub);
            }

            sessionCookie.ClearSessionId();
            signOutMessageCookie.Clear(id);
            
            ClearAuthenticationCookies();
            SignOutOfExternalIdP();

            if (user != null && user.Identity.IsAuthenticated)
            {
                await this.userService.SignOutAsync(user);

                var message = signOutMessageCookie.Read(id);
                eventService.RaiseLogoutEvent(user, id, message);
            }

            return await RenderLoggedOutPage(id);
        }

        private IHttpActionResult SignInAndRedirect(SignInMessage signInMessage, string signInMessageId, AuthenticateResult authResult, bool? rememberMe = null)
        {
            IssueAuthenticationCookie(signInMessageId, authResult, rememberMe);
            sessionCookie.IssueSessionId();

            var redirectUrl = GetRedirectUrl(signInMessage, authResult);
            Logger.InfoFormat("redirecting to: {0}", redirectUrl);
            return Redirect(redirectUrl);
        }

        private void IssueAuthenticationCookie(string signInMessageId, AuthenticateResult authResult, bool? rememberMe = null)
        {
            if (authResult == null) throw new ArgumentNullException("authResult");

            Logger.InfoFormat("issuing cookie{0}", authResult.IsPartialSignIn ? " (partial login)" : "");

            var props = new Microsoft.Owin.Security.AuthenticationProperties();

            var id = authResult.User.Identities.First();
            if (authResult.IsPartialSignIn)
            {
                // add claim so partial redirect can return here to continue login
                // we need a random ID to resume, and this will be the query string
                // to match a claim added. the claim added will be the original 
                // signIn ID. 
                var resumeId = CryptoRandom.CreateUniqueId();

                var resumeLoginUrl = context.GetPartialLoginResumeUrl(resumeId);
                var resumeLoginClaim = new Claim(Constants.ClaimTypes.PartialLoginReturnUrl, resumeLoginUrl);
                id.AddClaim(resumeLoginClaim);
                id.AddClaim(new Claim(GetClaimTypeForResumeId(resumeId), signInMessageId));
            }
            else
            {
                signInMessageCookie.Clear(signInMessageId);
            }

            if (!authResult.IsPartialSignIn)
            {
                // don't issue persistnt cookie if it's a partial signin
                if (rememberMe == true ||
                    (rememberMe != false && this.options.AuthenticationOptions.CookieOptions.IsPersistent))
                {
                    // only issue persistent cookie if user consents (rememberMe == true) or
                    // if server is configured to issue persistent cookies and user has not explicitly
                    // denied the rememberMe (false)
                    // if rememberMe is null, then user was not prompted for rememberMe
                    props.IsPersistent = true;
                    if (rememberMe == true)
                    {
                        var expires = DateTimeHelper.UtcNow.Add(options.AuthenticationOptions.CookieOptions.RememberMeDuration);
                        props.ExpiresUtc = new DateTimeOffset(expires);
                    }
                }
            }

            ClearAuthenticationCookies();
            
            context.Authentication.SignIn(props, id);
        }

        private static string GetClaimTypeForResumeId(string resume)
        {
            return String.Format(Constants.ClaimTypes.PartialLoginResumeId, resume);
        }

        private Uri GetRedirectUrl(SignInMessage signInMessage, AuthenticateResult authResult)
        {
            if (signInMessage == null) throw new ArgumentNullException("signInMessage");
            if (authResult == null) throw new ArgumentNullException("authResult");

            if (authResult.IsPartialSignIn)
            {
                var path = authResult.PartialSignInRedirectPath;
                if (path.StartsWith("~/"))
                {
                    path = path.Substring(2);
                    path = Request.GetIdentityServerBaseUrl() + path;
                }
                var host = new Uri(context.GetIdentityServerHost());
                return new Uri(host, path);
            }
            else
            {
                return new Uri(signInMessage.ReturnUrl);
            }
        }

        private void ClearAuthenticationCookies()
        {
            context.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);
        }

        private void SignOutOfExternalIdP()
        {
            // look for idp claim other than IdSvr
            // if present, then signout of it
            var user = User as ClaimsPrincipal;
            if (user != null && user.Identity.IsAuthenticated)
            {
                var idp = user.GetIdentityProvider();
                if (idp != Constants.BuiltInIdentityProvider)
                {
                    context.Authentication.SignOut(idp);
                }
            }
        }

        private async Task<IHttpActionResult> RenderLoginPage(SignInMessage message, string signInMessageId, string errorMessage = null, string username = null, bool rememberMe = false)
        {
            if (message == null) throw new ArgumentNullException("message");

            username = username ?? lastUsernameCookie.GetValue();

            var idpRestrictions = await clientStore.GetIdentityProviderRestrictionsAsync(message.ClientId);
            var providers = context.GetExternalAuthenticationProviders(idpRestrictions);
            var providerLinks = context.GetLinksFromProviders(providers, signInMessageId);
            var visibleLinks = providerLinks.FilterHiddenLinks();

            if (errorMessage != null)
            {
                Logger.InfoFormat("rendering login page with error message: {0}", errorMessage);
            }
            else
            {
                if (options.AuthenticationOptions.EnableLocalLogin == false)
                {
                    string url = null;

                    if (providerLinks.Count() == 1)
                    {
                        Logger.Info("only one provider for client");
                        url = providerLinks.First().Href;
                    }
                    else if (visibleLinks.Count() == 1)
                    {
                        Logger.Info("only one visible provider");
                        url = visibleLinks.First().Href;
                    }

                    if (url.IsPresent())
                    {
                        Logger.InfoFormat("redirecting to provider URL: {0}", url);
                        return Redirect(url);
                    }
                }

                Logger.Info("rendering login page");
            }

            var loginPageLinks = options.AuthenticationOptions.LoginPageLinks.Render(Request.GetIdentityServerBaseUrl(), signInMessageId);

            var loginModel = new LoginViewModel
            {
                RequestId = context.GetRequestId(),
                SiteName = options.SiteName,
                SiteUrl = Request.GetIdentityServerBaseUrl(),
                CurrentUser = User.Identity.Name,
                ExternalProviders = visibleLinks,
                AdditionalLinks = loginPageLinks,
                ErrorMessage = errorMessage,
                LoginUrl = options.AuthenticationOptions.EnableLocalLogin ? Url.Route(Constants.RouteNames.Login, new { signin = signInMessageId }) : null,
                AllowRememberMe = options.AuthenticationOptions.CookieOptions.AllowRememberMe,
                RememberMe = options.AuthenticationOptions.CookieOptions.AllowRememberMe && rememberMe,
                LogoutUrl = Url.Route(Constants.RouteNames.Logout, null),
                AntiForgery = antiForgeryToken.GetAntiForgeryToken(),
                Username = username
            };

            return new LoginActionResult(viewService, loginModel, message);
        }

        private async Task<IHttpActionResult> RenderLogoutPromptPage(string id = null)
        {
            var clientName = await clientStore.GetClientName(signOutMessageCookie.Read(id));

            var logoutModel = new LogoutViewModel
            {
                SiteName = options.SiteName,
                SiteUrl = context.GetIdentityServerBaseUrl(),
                CurrentUser = User.Identity.Name,
                LogoutUrl = Url.Route(Constants.RouteNames.Logout, new { id = id }),
                AntiForgery = antiForgeryToken.GetAntiForgeryToken(),
                ClientName = clientName
            };
            return new LogoutActionResult(viewService, logoutModel);
        }

        private async Task<IHttpActionResult> RenderLoggedOutPage(string id)
        {
            Logger.Info("rendering logged out page");

            var baseUrl = context.GetIdentityServerBaseUrl();
            var iframeUrls = options.RenderProtocolUrls(baseUrl);

            var message = signOutMessageCookie.Read(id);
            var redirectUrl = message != null ? message.ReturnUrl : null;
            var clientName = await clientStore.GetClientName(message);
            
            var loggedOutModel = new LoggedOutViewModel
            {
                SiteName = options.SiteName,
                SiteUrl = baseUrl,
                IFrameUrls = iframeUrls,
                ClientName = clientName,
                RedirectUrl = redirectUrl
            };
            return new LoggedOutActionResult(viewService, loggedOutModel);
        }

        private IHttpActionResult RenderErrorPage(string message = null)
        {
            message = message ?? localizationService.GetMessage(MessageIds.UnexpectedError);
            var errorModel = new ErrorViewModel
            {
                RequestId = context.GetRequestId(),
                SiteName = this.options.SiteName,
                SiteUrl = context.GetIdentityServerBaseUrl(),
                ErrorMessage = message
            };
            var errorResult = new ErrorActionResult(viewService, errorModel);
            return errorResult;
        }
    }
}