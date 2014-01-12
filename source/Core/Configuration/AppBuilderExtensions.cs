
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Microsoft.Owin.Extensions;

namespace Owin
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseIdentityServerCore(this IAppBuilder app, IdentityServerCoreOptions options)
        {
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, "Thinktecture.IdentityServer.Core.Authentication.Assets")
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets/libs/fonts"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, "Thinktecture.IdentityServer.Core.Authentication.Assets.libs.bootstrap.fonts")
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            app.UseWebApi(WebApiConfig.Configure());


            return app;
        }


    }
}
