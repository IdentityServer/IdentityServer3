using Host.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Text;
using IdentityServer3.Core.Extensions;

namespace Owin
{
    public static class SameSiteOpenIdConnectAuthenticationExtensions
    {
        public static IAppBuilder UseSameSiteOpenIdConnectAuthentication(this IAppBuilder app, OpenIdConnectAuthenticationOptions openIdConnectOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            if (openIdConnectOptions == null)
            {
                throw new ArgumentNullException("openIdConnectOptions");
            }

            return app.Use(typeof(SameSiteOpenIdConnectAuthenticationMiddleware), app, openIdConnectOptions);
        }
    }
    
    class SameSiteOpenIdConnectAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
    {
        private readonly ILogger _logger;

        public SameSiteOpenIdConnectAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, OpenIdConnectAuthenticationOptions options) : base(next, app, options)
        {
            _logger = app.CreateLogger<SameSiteOpenIdConnectAuthenticationMiddleware>();
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new SameSiteOpenIdConnectAuthenticationHandler(_logger);
        }
    }

    class SameSiteOpenIdConnectAuthenticationHandler : OpenIdConnectAuthenticationHandler
    {
        private const string NonceProperty = "N";
        private readonly ILogger _logger;

        public SameSiteOpenIdConnectAuthenticationHandler(ILogger logger) : base(logger)
        {
            _logger = logger;
        }

        protected override void RememberNonce(OpenIdConnectMessage message, string nonce)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (nonce == null)
            {
                throw new ArgumentNullException("nonce");
            }

            AuthenticationProperties properties = new AuthenticationProperties();
            properties.Dictionary.Add(NonceProperty, nonce);
            //Response.Cookies.Append(
            //    GetNonceKey(nonce),
            //    Convert.ToBase64String(Encoding.UTF8.GetBytes(Options.StateDataFormat.Protect(properties))),
            //    new CookieOptions
            //    {
            //        HttpOnly = true,
            //        Secure = Request.IsSecure
            //    });

            var key = GetNonceKey(nonce);
            var value = Convert.ToBase64String(Encoding.UTF8.GetBytes(Options.StateDataFormat.Protect(properties)));
            Context.Environment.AppendCookie(key, value);
        }

        protected override string RetrieveNonce(OpenIdConnectMessage message)
        {
            if (message == null)
            {
                return null;
            }

            string nonceKey = GetNonceKey(message.Nonce);
            if (nonceKey == null)
            {
                return null;
            }

            string nonceCookie = Request.Cookies[nonceKey];
            if (nonceCookie != null)
            {
                //var cookieOptions = new CookieOptions
                //{
                //    HttpOnly = true,
                //    Secure = Request.IsSecure
                //};

                //Response.Cookies.Delete(nonceKey, cookieOptions);
                Context.Environment.DeleteCookie(nonceKey);
            }

            if (string.IsNullOrWhiteSpace(nonceCookie))
            {
                _logger.WriteWarning("The nonce cookie was not found.");
                return null;
            }

            string nonce = null;
            AuthenticationProperties nonceProperties = Options.StateDataFormat.Unprotect(Encoding.UTF8.GetString(Convert.FromBase64String(nonceCookie)));
            if (nonceProperties != null)
            {
                nonceProperties.Dictionary.TryGetValue(NonceProperty, out nonce);
            }
            else
            {
                _logger.WriteWarning("Failed to un-protect the nonce cookie.");
            }

            return nonce;
        }
    }
}
