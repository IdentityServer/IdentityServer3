using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core.Lifetime;
using Microsoft.Owin;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Microsoft.Owin.Security;
using AuthenticateResult = Thinktecture.IdentityServer.Core.Models.AuthenticateResult;

namespace Thinktecture.IdentityServer.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a means to login to Identity Server from custom code running on the host.
    /// This is particularly useful for scenarios where you want to log a user in a way that is
    /// not directly supported by IdentityServer, for example IdP-initiated SSO.
    /// </summary>
    public class IdentityServerLogin
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IOwinContext owinContext;

        private readonly LifetimeScope autofacScope;

        public IdentityServerLogin(IDictionary<string, object> environment)
        {
            this.owinContext = new OwinContext(environment);
            this.autofacScope = GetAutofacScope(this.owinContext);
        }

        /// <summary>
        /// This will validate the externalIdentity against the UserService and will set the appropriate authentication cookies
        /// </summary>
        /// <param name="externalIdentity"></param>
        /// <returns></returns>
        public async Task<AuthenticateResult> LogInToIdentityServer(ExternalIdentity externalIdentity)
        {
            // Most of the code in this method is duplicated in AuthenticationController.LoginExternalCallback
            // However, due to the different use case it is tricky to refactor this code to be shared

            Logger.InfoFormat("custom login to IdSrv with external user provider: {0}, provider ID: {1}", externalIdentity.Provider, externalIdentity.ProviderId);
            var userService = GetUserService();
            var authResult = await userService.AuthenticateExternalAsync(externalIdentity, new SignInMessage());

            if (authResult == null)
            {
                Logger.Warn("custom login to IdSrv: user service failed to authenticate external identity");
                throw new Exception("The user service failed to authenticate external identity"); // TODO: There may be a more approprite exception to use?
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("custom login to IdSrv: user service returned error message: {0}", authResult.ErrorMessage);
                return authResult;
            }

            Logger.Info("custom login to IdSrv: External identity successfully validated by user service");

            // Get IdentityServer to set an authentication cookie
            SetTheIdSrvAuthCookie(authResult);
            this.GetTheSessionCookie().IssueSessionId();
            Logger.Info("custom login to IdSrv: Completed issuing session cookie");

            return authResult;
        }

        /// <summary>
        /// This does the mechanics of getting IdSrv to log you in and set an auth cookie
        /// Basically copy and paste from the auth controller
        /// This does deliberately not allow persistent cookies
        /// </summary>
        private void SetTheIdSrvAuthCookie(AuthenticateResult p)
        {
            var user = IdentityServerPrincipal.CreateFromPrincipal(p.User, Constants.PrimaryAuthenticationType);
            owinContext.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);

            var props = new AuthenticationProperties();
            var id = user.Identities.First();

            owinContext.Authentication.SignIn(props, id);
        }

        private SessionCookie GetTheSessionCookie()
        {
            var sessionCookie = autofacScope.GetService(typeof(SessionCookie)) as SessionCookie;
            if (sessionCookie == null)
            {
                Logger.Warn("custom user login failed to retrieve the SessionCookie from the dependency resolver");
                throw new Exception("Failed to get the SessionCookie from the dependency resolver");
            }
            return sessionCookie;
        }

        /// <summary>
        /// Horrible hack to get the IUserService
        /// Would like to replace with a cleaner way but not sure how that would be possible
        /// </summary>
        /// <returns></returns>
        private IUserService GetUserService()
        {
            var userService = autofacScope.GetService(typeof(IUserService)) as IUserService;
            if (userService == null)
            {
                Logger.Warn("custom user login failed to retrieve the IUserService from the dependency resolver");
                throw new Exception("Failed to get IUserService from the dependency resolver");
            }
            return userService;
        }

        private static LifetimeScope GetAutofacScope(IOwinContext owinContext)
        {
            var autofacScope = owinContext.Environment["idsrv:AutofacScope"] as LifetimeScope;

            if (autofacScope == null)
            {
                Logger.Warn("custom user login failed to retrieve autofac from the IOwinContext");
                throw new Exception("Unable to get the autofacscope");
            }
            return autofacScope;
        }
    }
}
