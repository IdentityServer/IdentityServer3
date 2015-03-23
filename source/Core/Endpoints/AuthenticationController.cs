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
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Events.Base;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.ViewModels;
using AuthenticateResult = Thinktecture.IdentityServer.Core.Models.AuthenticateResult;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ErrorPageFilter]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    [HostAuthentication(Constants.PRIMARY_AUTHENTICATION_TYPE)]
    internal class AuthenticationController : ApiController
    {
        public const int MAX_INPUT_PARAM_LENGTH = 100;

        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IOwinContext _context;
        private readonly IViewService _viewService;
        private readonly IUserService _userService;
        private readonly IdentityServerOptions _options;
        private readonly IClientStore _clientStore;
        private readonly IEventService _eventService;
        private readonly ILocalizationService _localizationService;
        private readonly SessionCookie _sessionCookie;
        private readonly MessageCookie<SignInMessage> _signInMessageCookie;
        private readonly MessageCookie<SignOutMessage> _signOutMessageCookie;
        private readonly LastUserNameCookie _lastUserNameCookie;
        private readonly AntiForgeryToken _antiForgeryToken;

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
            _context = new OwinContext(owin.Environment);
            _viewService = viewService;
            _userService = userService;
            _options = idSvrOptions;
            _clientStore = clientStore;
            _eventService = eventService;
            _localizationService = localizationService;
            _sessionCookie = sessionCookie;
            _signInMessageCookie = signInMessageCookie;
            _signOutMessageCookie = signOutMessageCookie;
            _lastUserNameCookie = lastUsernameCookie;
            _antiForgeryToken = antiForgeryToken;
        }

        [Route(Constants.RoutePaths.LOGIN, Name = Constants.RouteNames.LOGIN)]
        [HttpGet]
        public async Task<IHttpActionResult> Login(string signin)
        {
            Logger.Info("Login page requested");

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            if (signin.Length > MAX_INPUT_PARAM_LENGTH)
            {
                Logger.Error("Signin parameter passed was larger than max length");
                return RenderErrorPage();
            }

            var signInMessage = _signInMessageCookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            Logger.DebugFormat("signin message passed to login: {0}", JsonConvert.SerializeObject(signInMessage, Formatting.Indented));

            var authResult = await _userService.PreAuthenticateAsync(signInMessage);
            if (authResult != null)
            {
                if (authResult.IsError)
                {
                    Logger.WarnFormat("user service returned an error message: {0}", authResult.ErrorMessage);
                    
                    _eventService.RaisePreLoginFailureEvent(signin, signInMessage, authResult.ErrorMessage);
                    
                    return RenderErrorPage(authResult.ErrorMessage);
                }

                Logger.Info("user service returned a login result");

                _eventService.RaisePreLoginSuccessEvent(signin, signInMessage, authResult);
                
                return SignInAndRedirect(signInMessage, signin, authResult);
            }

            if (signInMessage.IdP.IsPresent())
            {
                Logger.InfoFormat("identity provider requested, redirecting to: {0}", signInMessage.IdP);
                return Redirect(_context.GetExternalProviderLoginUrl(signInMessage.IdP, signin));
            }

            return await RenderLoginPage(signInMessage, signin);
        }

        [Route(Constants.RoutePaths.LOGIN)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> LoginLocal(string signin, LoginCredentials model)
        {
            Logger.Info("Login page submitted");

            if (_options.AuthenticationOptions.EnableLocalLogin == false)
            {
                Logger.Warn("EnableLocalLogin disabled -- returning 405 MethodNotAllowed");
                return StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            if (signin.Length > MAX_INPUT_PARAM_LENGTH)
            {
                Logger.Error("Signin parameter passed was larger than max length");
                return RenderErrorPage();
            }
            
            var signInMessage = _signInMessageCookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            if (!(await IsLocalLoginAllowedForClient(signInMessage)))
            {
                Logger.ErrorFormat("Login not allowed for client {0}", signInMessage.ClientId);
                return RenderErrorPage();
            }

            if (model == null)
            {
                Logger.Error("no data submitted");
                return await RenderLoginPage(signInMessage, signin, _localizationService.GetMessage(MessageIds.INVALID_USERNAME_OR_PASSWORD));
            }

            if (String.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", _localizationService.GetMessage(MessageIds.USERNAME_REQUIRED));
            }
            
            if (String.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", _localizationService.GetMessage(MessageIds.PASSWORD_REQUIRED));
            }

            model.RememberMe = _options.AuthenticationOptions.CookieOptions.CalculateRememberMeFromUserInput(model.RememberMe);

            if (!ModelState.IsValid)
            {
                Logger.Warn("validation error: username or password missing");
                return await RenderLoginPage(signInMessage, signin, ModelState.GetError(), model.Username, model.RememberMe == true);
            }

            if (model.Username.Length > MAX_INPUT_PARAM_LENGTH || model.Password.Length > MAX_INPUT_PARAM_LENGTH)
            {
                Logger.Error("username or password submitted beyond allowed length");
                return await RenderLoginPage(signInMessage, signin);
            }
            
            var authResult = await _userService.AuthenticateLocalAsync(model.Username, model.Password, signInMessage);
            if (authResult == null)
            {
                Logger.WarnFormat("user service indicated incorrect username or password for username: {0}", model.Username);
                
                var errorMessage = _localizationService.GetMessage(MessageIds.INVALID_USERNAME_OR_PASSWORD);
                _eventService.RaiseLocalLoginFailureEvent(model.Username, signin, signInMessage, errorMessage);
                
                return await RenderLoginPage(signInMessage, signin, errorMessage, model.Username, model.RememberMe == true);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned an error message: {0}", authResult.ErrorMessage);

                _eventService.RaiseLocalLoginFailureEvent(model.Username, signin, signInMessage, authResult.ErrorMessage);
                
                return await RenderLoginPage(signInMessage, signin, authResult.ErrorMessage, model.Username, model.RememberMe == true);
            }

            Logger.Info("Login credentials successfully validated by user service");

            _eventService.RaiseLocalLoginSuccessEvent(model.Username, signin, signInMessage, authResult);

            _lastUserNameCookie.SetValue(model.Username);

            return SignInAndRedirect(signInMessage, signin, authResult, model.RememberMe);
        }

        [Route(Constants.RoutePaths.LOGIN_EXTERNAL, Name = Constants.RouteNames.LOGIN_EXTERNAL)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginExternal(string signin, string provider)
        {
            Logger.InfoFormat("External login requested for provider: {0}", provider);

            if (provider.IsMissing())
            {
                Logger.Error("No provider passed");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_EXTERNAL_PROVIDER));
            }

            if (provider.Length > MAX_INPUT_PARAM_LENGTH)
            {
                Logger.Error("Provider parameter passed was larger than max length");
                return RenderErrorPage();
            }

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            if (signin.Length > MAX_INPUT_PARAM_LENGTH)
            {
                Logger.Error("Signin parameter passed was larger than max length");
                return RenderErrorPage();
            }

            var signInMessage = _signInMessageCookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            if (!(await _clientStore.IsValidIdentityProviderAsync(signInMessage.ClientId, provider)))
            {
                var msg = String.Format("External login error: provider {0} not allowed for client: {1}", provider, signInMessage.ClientId);
                Logger.ErrorFormat(msg);
                _eventService.RaiseFailureEndpointEvent(EventConstants.EndpointNames.AUTHENTICATE, msg);
                return RenderErrorPage();
            }
            
            if (_context.IsValidExternalAuthenticationProvider(provider) == false)
            {
                var msg = String.Format("External login error: provider requested {0} is not a configured external provider", provider);
                Logger.ErrorFormat(msg);
                _eventService.RaiseFailureEndpointEvent(EventConstants.EndpointNames.AUTHENTICATE, msg);
                return RenderErrorPage();
            }

            var authProp = new AuthenticationProperties
            {
                RedirectUri = Url.Route(Constants.RouteNames.LOGIN_EXTERNAL_CALLBACK, null)
            };

            Logger.Info("Triggering challenge for external identity provider");

            // add the id to the dictionary so we can recall the cookie id on the callback
            authProp.Dictionary.Add(Constants.Authentication.SIGNIN_ID, signin);
            authProp.Dictionary.Add(Constants.Authentication.KATANA_AUTHENTICATION_TYPE, provider);
            _context.Authentication.Challenge(authProp, provider);
            
            return Unauthorized();
        }

        [Route(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK, Name = Constants.RouteNames.LOGIN_EXTERNAL_CALLBACK)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginExternalCallback(string error = null)
        {
            Logger.Info("Callback invoked from external identity provider");
            
            if (error.IsPresent())
            {
                if (error.Length > MAX_INPUT_PARAM_LENGTH) error = error.Substring(0, MAX_INPUT_PARAM_LENGTH);

                Logger.ErrorFormat("External identity provider returned error: {0}", error);
                _eventService.RaiseExternalLoginErrorEvent(error);
                return RenderErrorPage(String.Format(_localizationService.GetMessage(MessageIds.EXTERNAL_PROVIDER_ERROR), error));
            }

            var signInId = await _context.GetSignInIdFromExternalProvider();
            if (signInId.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            var signInMessage = _signInMessageCookie.Read(signInId);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            var user = await _context.GetIdentityFromExternalProvider();
            if (user == null)
            {
                Logger.Error("no identity from external identity provider");
                return await RenderLoginPage(signInMessage, signInId, _localizationService.GetMessage(MessageIds.NO_MATCHING_EXTERNAL_ACCOUNT));
            }

            var externalIdentity = ExternalIdentity.FromClaims(user.Claims);
            if (externalIdentity == null)
            {
                var claims = user.Claims.Select(x => new { x.Type, x.Value });
                Logger.ErrorFormat("no subject or unique identifier claims from external identity provider. Claims provided:\r\n{0}", LogSerializer.Serialize(claims));
                return await RenderLoginPage(signInMessage, signInId, _localizationService.GetMessage(MessageIds.NO_MATCHING_EXTERNAL_ACCOUNT));
            }

            Logger.InfoFormat("external user provider: {0}, provider ID: {1}", externalIdentity.Provider, externalIdentity.ProviderId);

            var authResult = await _userService.AuthenticateExternalAsync(externalIdentity, signInMessage);
            if (authResult == null)
            {
                Logger.Warn("user service failed to authenticate external identity");
                
                var msg = _localizationService.GetMessage(MessageIds.NO_MATCHING_EXTERNAL_ACCOUNT);
                _eventService.RaiseExternalLoginFailureEvent(externalIdentity, signInId, signInMessage, msg);
                
                return await RenderLoginPage(signInMessage, signInId, msg);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned error message: {0}", authResult.ErrorMessage);

                _eventService.RaiseExternalLoginFailureEvent(externalIdentity, signInId, signInMessage, authResult.ErrorMessage);
                
                return await RenderLoginPage(signInMessage, signInId, authResult.ErrorMessage);
            }

            Logger.Info("External identity successfully validated by user service");

            _eventService.RaiseExternalLoginSuccessEvent(externalIdentity, signInId, signInMessage, authResult);

            return SignInAndRedirect(signInMessage, signInId, authResult);
        }

        [Route(Constants.RoutePaths.RESUME_LOGIN_FROM_REDIRECT, Name = Constants.RouteNames.RESUME_LOGIN_FROM_REDIRECT)]
        [HttpGet]
        public async Task<IHttpActionResult> ResumeLoginFromRedirect(string resume)
        {
            Logger.Info("Callback requested to resume login from partial login");

            if (resume.IsMissing())
            {
                Logger.Error("no resumeId passed");
                return RenderErrorPage();
            }

            if (resume.Length > MAX_INPUT_PARAM_LENGTH)
            {
                Logger.Error("resumeId length longer than allowed length");
                return RenderErrorPage();
            }

            var user = await _context.GetIdentityFromPartialSignIn();
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

            var signInMessage = _signInMessageCookie.Read(signInId);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage(_localizationService.GetMessage(MessageIds.NO_SIGN_IN_COOKIE));
            }

            // check to see if the partial login has all the claim types needed to login
            AuthenticateResult result;
            if (Constants.AuthenticateResultClaimTypes.All(claimType => user.HasClaim(claimType)))
            {
                Logger.Info("Authentication claims found -- logging user in");
                
                // the user/subject was known, so pass thru (without the redirect claims)
                if (user.HasClaim(Constants.ClaimTypes.PARTIAL_LOGIN_RETURN_URL))
                {
                    user.RemoveClaim(user.FindFirst(Constants.ClaimTypes.PARTIAL_LOGIN_RETURN_URL));
                }
                if (user.HasClaim(Constants.ClaimTypes.EXTERNAL_PROVIDER_USER_ID))
                {
                    user.RemoveClaim(user.FindFirst(Constants.ClaimTypes.EXTERNAL_PROVIDER_USER_ID));
                }
                if (user.HasClaim(GetClaimTypeForResumeId(resume)))
                {
                    user.RemoveClaim(user.FindFirst(GetClaimTypeForResumeId(resume)));
                }
                
                result = new AuthenticateResult(new ClaimsPrincipal(user));

                _eventService.RaisePartialLoginCompleteEvent(user, signInId, signInMessage);
            }
            else
            {
                Logger.Info("Authentication claims not found -- looking for ExternalProviderUserId to call AuthenticateExternalAsync");
                
                // the user was not known, we need to re-execute AuthenticateExternalAsync
                // to obtain a subject to proceed
                var externalProviderClaim = user.FindFirst(Constants.ClaimTypes.EXTERNAL_PROVIDER_USER_ID);
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

                Logger.InfoFormat("external user provider: {0}, provider ID: {1}", externalId.Provider, externalId.ProviderId);
                
                result = await _userService.AuthenticateExternalAsync(externalId, signInMessage);

                if (result == null)
                {
                    Logger.Warn("user service failed to authenticate external identity");
                    
                    var msg = _localizationService.GetMessage(MessageIds.NO_MATCHING_EXTERNAL_ACCOUNT);
                    _eventService.RaiseExternalLoginFailureEvent(externalId, signInId, signInMessage, msg);
                    
                    return await RenderLoginPage(signInMessage, signInId, msg);
                }

                if (result.IsError)
                {
                    Logger.WarnFormat("user service returned error message: {0}", result.ErrorMessage);

                    _eventService.RaiseExternalLoginFailureEvent(externalId, signInId, signInMessage, result.ErrorMessage);
                    
                    return await RenderLoginPage(signInMessage, signInId, result.ErrorMessage);
                }

                Logger.Info("External identity successfully validated by user service");

                _eventService.RaiseExternalLoginSuccessEvent(externalId, signInId, signInMessage, result);
            }

            return SignInAndRedirect(signInMessage, signInId, result);
        }

        [Route(Constants.RoutePaths.LOGOUT, Name = Constants.RouteNames.LOGOUT_PROMPT)]
        [HttpGet]
        public async Task<IHttpActionResult> LogoutPrompt(string id = null)
        {
            if (id != null && id.Length > MAX_INPUT_PARAM_LENGTH)
            {
                Logger.Error("Logout prompt requested, but id param is longer than allowed length");
                return RenderErrorPage();
            }

            var user = (ClaimsPrincipal)User;
            if (user == null || user.Identity.IsAuthenticated == false)
            {
                // user is already logged out, so just trigger logout cleanup
                return await Logout(id);
            }

            var sub = user.GetSubjectId();
            Logger.InfoFormat("Logout prompt for subject: {0}", sub);

            var message = _signOutMessageCookie.Read(id);
            if (message != null && message.ClientId.IsPresent())
            {
                Logger.InfoFormat("SignOutMessage present (from client {0}), performing logout", message.ClientId);
                return await Logout(id);
            }

            if (!_options.AuthenticationOptions.EnableSignOutPrompt)
            {
                Logger.InfoFormat("EnableSignOutPrompt set to false, performing logout");
                return await Logout(id);
            }

            Logger.InfoFormat("EnableSignOutPrompt set to true, rendering logout prompt");
            return RenderLogoutPromptPage();
        }

        [Route(Constants.RoutePaths.LOGOUT, Name = Constants.RouteNames.LOGOUT)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> Logout(string id = null)
        {
            Logger.Info("Logout endpoint submitted");

            if (id != null && id.Length > MAX_INPUT_PARAM_LENGTH)
            {
                Logger.Error("id param is longer than allowed length");
                return RenderErrorPage();
            }
            
            var user = (ClaimsPrincipal)User;
            if (user != null && user.Identity.IsAuthenticated)
            {
                var sub = user.GetSubjectId();
                Logger.InfoFormat("Logout requested for subject: {0}", sub);
            }

            Logger.Info("Clearing cookies");

            _sessionCookie.ClearSessionId();
            _signOutMessageCookie.Clear(id);
            
            ClearAuthenticationCookies();
            SignOutOfExternalIdP();

            if (user != null && user.Identity.IsAuthenticated)
            {
                await _userService.SignOutAsync(user);

                var message = _signOutMessageCookie.Read(id);
                _eventService.RaiseLogoutEvent(user, id, message);
            }

            return await RenderLoggedOutPage(id);
        }

        private IHttpActionResult SignInAndRedirect(SignInMessage signInMessage, string signInMessageId, AuthenticateResult authResult, bool? rememberMe = null)
        {
            ClearAuthenticationCookies();
            IssueAuthenticationCookie(signInMessageId, authResult, rememberMe);

            var redirectUrl = GetRedirectUrl(signInMessage, authResult);
            Logger.InfoFormat("redirecting to: {0}", redirectUrl);
            return Redirect(redirectUrl);
        }

        private void IssueAuthenticationCookie(string signInMessageId, AuthenticateResult authResult, bool? rememberMe = null)
        {
            if (authResult == null) throw new ArgumentNullException("authResult");

            Logger.InfoFormat("issuing cookie{0}", authResult.IsPartialSignIn ? " (partial login)" : "");

            var props = new AuthenticationProperties();

            var id = authResult.User.Identities.First();
            if (authResult.IsPartialSignIn)
            {
                // add claim so partial redirect can return here to continue login
                // we need a random ID to resume, and this will be the query string
                // to match a claim added. the claim added will be the original 
                // signIn ID. 
                var resumeId = CryptoRandom.CreateUniqueId();

                var resumeLoginUrl = _context.GetPartialLoginResumeUrl(resumeId);
                var resumeLoginClaim = new Claim(Constants.ClaimTypes.PARTIAL_LOGIN_RETURN_URL, resumeLoginUrl);
                id.AddClaim(resumeLoginClaim);
                id.AddClaim(new Claim(GetClaimTypeForResumeId(resumeId), signInMessageId));
            }
            else
            {
                _signInMessageCookie.Clear(signInMessageId);
                _sessionCookie.IssueSessionId(rememberMe);
            }

            if (!authResult.IsPartialSignIn)
            {
                // don't issue persistnt cookie if it's a partial signin
                if (rememberMe == true ||
                    (rememberMe != false && _options.AuthenticationOptions.CookieOptions.IsPersistent))
                {
                    // only issue persistent cookie if user consents (rememberMe == true) or
                    // if server is configured to issue persistent cookies and user has not explicitly
                    // denied the rememberMe (false)
                    // if rememberMe is null, then user was not prompted for rememberMe
                    props.IsPersistent = true;
                    if (rememberMe == true)
                    {
                        var expires = DateTimeHelper.UtcNow.Add(_options.AuthenticationOptions.CookieOptions.RememberMeDuration);
                        props.ExpiresUtc = new DateTimeOffset(expires);
                    }
                }
            }

            _context.Authentication.SignIn(props, id);
        }

        private static string GetClaimTypeForResumeId(string resume)
        {
            return String.Format(Constants.ClaimTypes.PARTIAL_LOGIN_RESUME_ID, resume);
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
                var host = new Uri(_context.GetIdentityServerHost());
                return new Uri(host, path);
            }
            return new Uri(signInMessage.ReturnUrl);
        }

        private void ClearAuthenticationCookies()
        {
            _context.Authentication.SignOut(
                Constants.PRIMARY_AUTHENTICATION_TYPE,
                Constants.EXTERNAL_AUTHENTICATION_TYPE,
                Constants.PARTIAL_SIGN_IN_AUTHENTICATION_TYPE);
        }

        private void SignOutOfExternalIdP()
        {
            // look for idp claim other than IdSvr
            // if present, then signout of it
            var user = User as ClaimsPrincipal;
            if (user != null && user.Identity.IsAuthenticated)
            {
                var idp = user.GetIdentityProvider();
                if (idp != Constants.BUILT_IN_IDENTITY_PROVIDER)
                {
                    _context.Authentication.SignOut(idp);
                }
            }
        }

        async Task<bool> IsLocalLoginAllowedForClient(SignInMessage message)
        {
            if (message != null && message.ClientId.IsPresent())
            {
                var client = await _clientStore.FindClientByIdAsync(message.ClientId);
                if (client != null)
                {
                    return client.EnableLocalLogin;
                }
            }

            return true;
        }

        private async Task<IHttpActionResult> RenderLoginPage(SignInMessage message, string signInMessageId, string errorMessage = null, string username = null, bool rememberMe = false)
        {
            if (message == null) throw new ArgumentNullException("message");

            username = GetUserNameForLoginPage(message, username);

            var isLocalLoginAllowedForClient = await IsLocalLoginAllowedForClient(message);
            var isLocalLoginAllowed = isLocalLoginAllowedForClient && _options.AuthenticationOptions.EnableLocalLogin;

            var idpRestrictions = await _clientStore.GetIdentityProviderRestrictionsAsync(message.ClientId);
            var providers = _context.GetExternalAuthenticationProviders(idpRestrictions);
            var providerLinks = _context.GetLinksFromProviders(providers, signInMessageId);
            var visibleLinks = providerLinks.FilterHiddenLinks();

            if (errorMessage != null)
            {
                Logger.InfoFormat("rendering login page with error message: {0}", errorMessage);
            }
            else
            {
                if (isLocalLoginAllowed == false)
                {
                    if (_options.AuthenticationOptions.EnableLocalLogin)
                    {
                        Logger.Info("local login disabled");
                    }
                    if (isLocalLoginAllowedForClient)
                    {
                        Logger.Info("local login disabled for the client");
                    }

                    string url = null;

                    if (!providerLinks.Any())
                    {
                        Logger.Info("no providers registered for client");
                        return RenderErrorPage();
                    }
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

            var loginPageLinks = _options.AuthenticationOptions.LoginPageLinks.Render(Request.GetIdentityServerBaseUrl(), signInMessageId);

            var loginModel = new LoginViewModel
            {
                RequestId = _context.GetRequestId(),
                SiteName = _options.SiteName,
                SiteUrl = Request.GetIdentityServerBaseUrl(),
                ExternalProviders = visibleLinks,
                AdditionalLinks = loginPageLinks,
                ErrorMessage = errorMessage,
                LoginUrl = isLocalLoginAllowed ? Url.Route(Constants.RouteNames.LOGIN, new { signin = signInMessageId }) : null,
                AllowRememberMe = _options.AuthenticationOptions.CookieOptions.AllowRememberMe,
                RememberMe = _options.AuthenticationOptions.CookieOptions.AllowRememberMe && rememberMe,
                CurrentUser = _context.GetCurrentUserDisplayName(),
                LogoutUrl = _context.GetIdentityServerLogoutUrl(),
                AntiForgery = _antiForgeryToken.GetAntiForgeryToken(),
                Username = username
            };

            return new LoginActionResult(_viewService, loginModel, message);
        }

        private string GetUserNameForLoginPage(SignInMessage message, string username)
        {
            if (username.IsMissing() && message.LoginHint.IsPresent())
            {
                if (_options.AuthenticationOptions.EnableLoginHint)
                {
                    Logger.InfoFormat("Using LoginHint for username: {0}", message.LoginHint);
                    username = message.LoginHint;
                }
                else
                {
                    Logger.Warn("Not using LoginHint because EnableLoginHint is false");
                }
            }

            var lastUsernameCookieValue = _lastUserNameCookie.GetValue();
            if (username.IsMissing() && lastUsernameCookieValue.IsPresent())
            {
                Logger.InfoFormat("Using LastUserNameCookie value for username: {0}", lastUsernameCookieValue);
                username = lastUsernameCookieValue;
            }
            return username;
        }

        private IHttpActionResult RenderLogoutPromptPage()
        {
            var logoutModel = new LogoutViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = _context.GetIdentityServerBaseUrl(),
                CurrentUser = _context.GetCurrentUserDisplayName(),
                LogoutUrl = _context.GetIdentityServerLogoutUrl(),
                AntiForgery = _antiForgeryToken.GetAntiForgeryToken(),
            };

            return new LogoutActionResult(_viewService, logoutModel);
        }

        private async Task<IHttpActionResult> RenderLoggedOutPage(string id)
        {
            Logger.Info("rendering logged out page");

            var baseUrl = _context.GetIdentityServerBaseUrl();
            var iframeUrls = _options.RenderProtocolUrls(baseUrl);

            var message = _signOutMessageCookie.Read(id);
            var redirectUrl = message != null ? message.ReturnUrl : null;
            var clientName = await _clientStore.GetClientName(message);
            
            var loggedOutModel = new LoggedOutViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = baseUrl,
                IFrameUrls = iframeUrls,
                ClientName = clientName,
                RedirectUrl = redirectUrl,
                AutoRedirect = _options.AuthenticationOptions.EnablePostSignOutAutoRedirect,
                AutoRedirectDelay = _options.AuthenticationOptions.PostSignOutAutoRedirectDelay
            };
            return new LoggedOutActionResult(_viewService, loggedOutModel);
        }

        private IHttpActionResult RenderErrorPage(string message = null)
        {
            message = message ?? _localizationService.GetMessage(MessageIds.UNEXPECTED_ERROR);
            var errorModel = new ErrorViewModel
            {
                RequestId = _context.GetRequestId(),
                SiteName = _options.SiteName,
                SiteUrl = _context.GetIdentityServerBaseUrl(),
                ErrorMessage = message,
                CurrentUser = _context.GetCurrentUserDisplayName(),
                LogoutUrl = _context.GetIdentityServerLogoutUrl(),
            };
            var errorResult = new ErrorActionResult(_viewService, errorModel);
            return errorResult;
        }
    }
}