using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Thinktecture.IdentityModel.Web;

namespace Thinktecture.IdentityModel.Oidc
{
    public class AuthorizeResponseEventArgs : EventArgs
    {
        public OidcAuthorizeResponse Response { get; set; }
        public bool Cancel { get; set; }
        public string RedirectUrl { get; set; }
    }
    public class TokenResponseEventArgs : EventArgs
    {
        public OidcTokenResponse Response { get; set; }
        public bool Cancel { get; set; }
        public string RedirectUrl { get; set; }
    }
    public class IdentityTokenValidatedEventArgs : EventArgs
    {
        public IEnumerable<Claim> Claims { get; set; }
        public bool Cancel { get; set; }
        public string RedirectUrl { get; set; }
    }
    public class UserInfoClaimsReceivedEventArgs : EventArgs
    {
        public IEnumerable<Claim> Claims { get; set; }
        public bool Cancel { get; set; }
        public string RedirectUrl { get; set; }
    }
    public class SessionTokenCreatedEventArgs : EventArgs
    {
        public SessionSecurityToken Token { get; set; }
    }
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public bool ExceptionHandled { get; set; }
    }
    
    public class OpenIdConnectAuthenticationModule : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.AddOnAuthenticateRequestAsync(BeginAuthenticateRequest, EndAuthenticateRequest);
            app.EndRequest += OnEndRequest;
        }

        public IAsyncResult BeginAuthenticateRequest(
            object sender, EventArgs e, AsyncCallback cb, object extraData)
        {
            var tcs = new TaskCompletionSource<object>(extraData);
            AuthenticateAsync(HttpContext.Current).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.SetException(t.Exception.InnerExceptions);
                }
                else
                {
                    tcs.SetResult(null);
                }
                if (cb != null) cb(tcs.Task);
            });
            return tcs.Task;
        }

        public void EndAuthenticateRequest(IAsyncResult result)
        {
            Task task = (Task)result;
            task.Wait();
        }

        public event EventHandler<AuthorizeResponseEventArgs> AuthorizeResponse;
        protected virtual void OnAuthorizeResponse(AuthorizeResponseEventArgs args)
        {
            if (AuthorizeResponse != null) 
            {
                AuthorizeResponse(this, args);
            }
        }
        public event EventHandler<TokenResponseEventArgs> TokenResponse;
        protected virtual void OnTokenResponse(TokenResponseEventArgs args)
        {
            if (TokenResponse != null)
            {
                TokenResponse(this, args);
            }
        }
        public event EventHandler<IdentityTokenValidatedEventArgs> IdentityTokenValidated;
        protected virtual void OnIdentityTokenValidated(IdentityTokenValidatedEventArgs args)
        {
            if (IdentityTokenValidated != null)
            {
                IdentityTokenValidated(this, args);
            }
        }
        public event EventHandler<UserInfoClaimsReceivedEventArgs> UserInfoClaimsReceived;
        protected virtual void OnUserInfoClaimsReceived(UserInfoClaimsReceivedEventArgs args)
        {
            if (UserInfoClaimsReceived != null)
            {
                UserInfoClaimsReceived(this, args);
            }
        }
        public event EventHandler<SessionTokenCreatedEventArgs> SessionSecurityTokenCreated;
        protected virtual void OnSessionSecurityTokenCreated(SessionTokenCreatedEventArgs args)
        {
            if (SessionSecurityTokenCreated != null)
            {
                SessionSecurityTokenCreated(this, args);
            }
        }
        public event EventHandler SignedIn;
        protected virtual void OnSignedIn()
        {
            if (SignedIn != null)
            {
                SignedIn(this, EventArgs.Empty);
            }
        }
        public event EventHandler<ErrorEventArgs> Error;
        protected virtual void OnError(ErrorEventArgs args)
        {
            if (Error != null)
            {
                Error(this, args);
            }
        }

        async Task AuthenticateAsync(HttpContext context)
        {
            var config = OidcClientConfigurationSection.Instance;

            var appRelativeCallbackUrl = config.AppRelativeCallbackUrl;
            if (context.Request.AppRelativeCurrentExecutionFilePath.Equals(appRelativeCallbackUrl, StringComparison.OrdinalIgnoreCase))
            {
                var authorizeErrorUrl = config.AuthorizeErrorRedirectUrl;
                var tokenUrl = config.Endpoints.Token;
                var userInfoUrl = config.Endpoints.UserInfo;
                var clientId = config.ClientId;
                var clientSecret = config.ClientSecret;
                var issuerName = config.IssuerName;
                var signingcert = X509.LocalMachine.TrustedPeople.SubjectDistinguishedName.Find(
                    config.SigningCertificate, false).First();
                var callUserInfoEndpoint = config.CallUserInfoEndpoint;

                // parse OIDC authorize response
                var response = OidcClient.HandleAuthorizeResponse(context.Request.QueryString);

                // event? callback recieved, pass response (allow cancel and need new url?)
                var authorizeResponseEventArgs = new AuthorizeResponseEventArgs { Response = response };
                OnAuthorizeResponse(authorizeResponseEventArgs);
                if (authorizeResponseEventArgs.Cancel)
                {
                    if (String.IsNullOrWhiteSpace(authorizeResponseEventArgs.RedirectUrl))
                    {
                        throw new ArgumentNullException("RedirectUrl");
                    }
                    context.Response.Redirect(authorizeResponseEventArgs.RedirectUrl);
                    return;
                }

                if (response.IsError)
                {
                    if (!String.IsNullOrWhiteSpace(authorizeErrorUrl))
                    {
                        if (!authorizeErrorUrl.Contains("?"))
                        {
                            authorizeErrorUrl += "?";
                        }
                        if (!authorizeErrorUrl.EndsWith("?"))
                        {
                            authorizeErrorUrl += "&";
                        }
                        authorizeErrorUrl += "error=" + response.Error;
                        context.Response.Redirect(authorizeErrorUrl);
                        return;
                    }

                    throw new InvalidOperationException("OpenID Connect Callback Error: " + response.Error + ". Handle the AuthorizeResponse event to handle authorization errors.");
                }

                try
                {
                    // read and parse state cookie
                    var cookie = new ProtectedCookie(ProtectionMode.MachineKey);
                    var storedState = cookie.Read("oidcstate");
                    ProtectedCookie.Delete("oidcstate");

                    var separator = storedState.IndexOf('_');
                    if (separator == -1)
                    {
                        throw new InvalidOperationException("state invalid.");
                    }

                    var state = storedState.Substring(0, separator);
                    var returnUrl = storedState.Substring(separator + 1);
                    
                    if (appRelativeCallbackUrl.StartsWith("~/"))
                    {
                        appRelativeCallbackUrl = appRelativeCallbackUrl.Substring(2);
                    }
                    var redirectUri = context.Request.GetApplicationUrl() + appRelativeCallbackUrl;

                    // validate state
                    if (response.State != state)
                    {
                        throw new InvalidOperationException("state invalid.");
                    }

                    // call token endpoint and retrieve id and access token (and maybe a refresh token)
                    var tokenResponse = await OidcClient.CallTokenEndpointAsync(
                        new Uri(tokenUrl),
                        new Uri(redirectUri),
                        response.Code,
                        clientId,
                        clientSecret);

                    // event -- token obtained event
                    var tokenResponseEventArgs = new TokenResponseEventArgs { Response = tokenResponse };
                    OnTokenResponse(tokenResponseEventArgs);
                    if (tokenResponseEventArgs.Cancel)
                    {
                        if (String.IsNullOrWhiteSpace(tokenResponseEventArgs.RedirectUrl))
                        {
                            throw new ArgumentNullException("RedirectUrl");
                        } 
                        
                        context.Response.Redirect(tokenResponseEventArgs.RedirectUrl);
                        return;
                    }

                    // validate identity token
                    var identityClaims = OidcClient.ValidateIdentityToken(
                        tokenResponse.IdentityToken,
                        issuerName,
                        clientId,
                        signingcert);
                    
                    // event -- identity token validated w/ claims
                    var identityTokenValidatedEventArgs = new IdentityTokenValidatedEventArgs { Claims = identityClaims };
                    OnIdentityTokenValidated(identityTokenValidatedEventArgs);
                    if (identityTokenValidatedEventArgs.Cancel)
                    {
                        if (String.IsNullOrWhiteSpace(identityTokenValidatedEventArgs.RedirectUrl))
                        {
                            throw new ArgumentNullException("RedirectUrl");
                        }

                        context.Response.Redirect(identityTokenValidatedEventArgs.RedirectUrl);
                        return;
                    }

                    var claims = identityTokenValidatedEventArgs.Claims.ToList();

                    if (callUserInfoEndpoint)
                    {
                        // retrieve user info data
                        var userInfoClaims = await OidcClient.GetUserInfoClaimsAsync(
                            new Uri(userInfoUrl),
                            tokenResponse.AccessToken);

                        // event -- profile loaded w/ claims
                        var userInfoClaimsReceivedEventArgs = new UserInfoClaimsReceivedEventArgs { Claims = userInfoClaims };
                        OnUserInfoClaimsReceived(userInfoClaimsReceivedEventArgs);
                        if (userInfoClaimsReceivedEventArgs.Cancel)
                        {
                            if (String.IsNullOrWhiteSpace(userInfoClaimsReceivedEventArgs.RedirectUrl))
                            {
                                throw new ArgumentNullException("RedirectUrl");
                            }

                            context.Response.Redirect(userInfoClaimsReceivedEventArgs.RedirectUrl);
                            return;
                        }

                        claims = userInfoClaimsReceivedEventArgs.Claims.ToList();
                    }

                    // create identity
                    claims.Add(new Claim("at", tokenResponse.AccessToken));
                    var id = new ClaimsIdentity(claims, "oidc");

                    if (!string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
                    {
                        id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
                    }

                    // create principal
                    var principal = new ClaimsPrincipal(id);
                    var transformedPrincipal = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager.Authenticate(string.Empty, principal);

                    // establish session
                    var sessionToken = new SessionSecurityToken(transformedPrincipal);

                    // event? raise session security token created
                    var sessionTokenCreatedEventArgs = new SessionTokenCreatedEventArgs { Token = sessionToken };
                    OnSessionSecurityTokenCreated(sessionTokenCreatedEventArgs);
                        
                    FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionToken);

                    // event? signed in -- pass in return URL and allow them to change
                    OnSignedIn();

                    // redirect local to return url
                    if (!string.IsNullOrWhiteSpace(returnUrl))
                    {
                        context.Response.Redirect(returnUrl);
                    }
                    else
                    {
                        context.Response.Redirect("~/");
                    }
                }
                catch (Exception ex)
                {
                    // event? general error/fail
                    var errorEventArgs = new ErrorEventArgs() { Exception = ex };
                    OnError(errorEventArgs);
                    if (!errorEventArgs.ExceptionHandled)
                    {
                        throw; 
                    }
                }
            }
        }

        void OnEndRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;

            if (context.Response.StatusCode == 401 &&
                !context.User.Identity.IsAuthenticated &&
                !context.Response.SuppressFormsAuthenticationRedirect)
            {
                var config = OidcClientConfigurationSection.Instance;

                var authorizeUrl = config.Endpoints.Authorize;
                var clientId = config.ClientId;
                var scopes = "openid " + config.Scope;
                var state = Guid.NewGuid().ToString("N");
                var returnUrl = context.Request.RawUrl;

                var appRelativeCallbackUrl = config.AppRelativeCallbackUrl;
                if (appRelativeCallbackUrl.StartsWith("~/"))
                {
                    appRelativeCallbackUrl = appRelativeCallbackUrl.Substring(2);
                }
                var redirectUri = context.Request.GetApplicationUrl() + appRelativeCallbackUrl;

                var authorizeUri = OidcClient.GetAuthorizeUrl(
                    new Uri(authorizeUrl),
                    new Uri(redirectUri),
                    clientId,
                    scopes,
                    state);

                var cookie = new ProtectedCookie(ProtectionMode.MachineKey);
                cookie.Write("oidcstate", state + "_" + returnUrl, DateTime.UtcNow.AddHours(1));

                context.Response.Redirect(authorizeUri.AbsoluteUri);
            }
        }

        public void Dispose()
        { }
    }
}