using Microsoft.Owin;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Views;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public class ValidateAntiForgeryTokenAttribute : PreventUnsupportedRequestMediaTypesAttribute
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public ValidateAntiForgeryTokenAttribute()
            : base(allowFormUrlEncoded:true)
        {
        }

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // firect check for 415
            await base.OnAuthorizationAsync(actionContext, cancellationToken);

            if (actionContext.Response == null)
            {
                await ValidateTokens(actionContext);
            }
        }

        private static async Task ValidateTokens(HttpActionContext actionContext)
        {
            var env = actionContext.Request.GetOwinEnvironment();

            var success = actionContext.Request.Method == HttpMethod.Post &&
                          actionContext.Request.Content.IsFormData();
            if (success)
            {
                // ReadAsByteArrayAsync buffers the request body stream
                // we then put the buffered copy into the owin context
                // so we can read it in the IsTokenValid API without 
                // disturbing the actual stream in the HttpRequestMessage
                // that WebAPI uses it later for model binding. #lame
                var bytes = await actionContext.Request.Content.ReadAsByteArrayAsync();
                var ms = new MemoryStream(bytes);
                ms.Seek(0, SeekOrigin.Begin);
                var ctx = new OwinContext(env);
                ctx.Request.Body = ms;

                success = await AntiForgeryTokenValidator.IsTokenValid(env);
            }

            if (!success)
            {
                Logger.ErrorFormat("AntiForgery validation failed -- returning error page");

                var options = env.ResolveDependency<IdentityServerOptions>();
                var viewSvc = env.ResolveDependency<IViewService>();
                var errorModel = new ErrorViewModel
                {
                    SiteName = options.SiteName,
                    SiteUrl = env.GetIdentityServerBaseUrl(),
                    ErrorMessage = Resources.Messages.UnexpectedError,
                };
                var errorResult = new ErrorActionResult(viewSvc, env, errorModel);
                actionContext.Response = await errorResult.GetResponseMessage();
            }
        }
    }
}
