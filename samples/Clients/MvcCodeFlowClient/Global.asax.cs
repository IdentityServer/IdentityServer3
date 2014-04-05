using System;
using System.IdentityModel.Tokens;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Thinktecture.IdentityModel.Oidc;
using Thinktecture.IdentityModel.Tokens;

namespace CodeFlowClient
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //OidcClientConfigurationSection.Instance.ClientId = "MyClientId";
            //OidcClientConfigurationSection.Instance.ClientId = "MySecret";

            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
        }

        void OpenIdConnectAuthenticationModule_AuthorizeResponse(object sender, AuthorizeResponseEventArgs args)
        {
            if (args.Response.IsError)
            {
                //args.Cancel = true;
                //args.RedirectUrl = "~/error";
            }
        }
        void OpenIdConnectAuthenticationModule_TokenResponse(object sender, TokenResponseEventArgs args)
        {
        }
        void OpenIdConnectAuthenticationModule_IdentityTokenValidated(object sender, IdentityTokenValidatedEventArgs args)
        {
        }
        void OpenIdConnectAuthenticationModule_UserInfoClaimsReceived(object sender, UserInfoClaimsReceivedEventArgs args)
        {
        }
        void OpenIdConnectAuthenticationModule_SessionSecurityTokenCreated(object sender, SessionTokenCreatedEventArgs args)
        {
        }
        void OpenIdConnectAuthenticationModule_SignedIn(object sender, EventArgs args)
        {
        }
        void OpenIdConnectAuthenticationModule_Error(object sender, ErrorEventArgs args)
        {
        }
    }
}