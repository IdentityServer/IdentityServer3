
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
using Autofac;

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

            var container = AutoFacConfig.Configure(options.Factory);

            app.Use(async (ctx, next) =>
            {
                // this creates a per-request, disposable scope
                using (var scope = container.BeginLifetimeScope(b =>
                {
                    // this makes owin context resolvable in the scope
                    b.RegisterInstance(ctx).As<IOwinContext>();
                }))
                {
                    // this makes scope available for downstream frameworks
                    ctx.Set<ILifetimeScope>("idsrv:AutofacScope", scope);
                    await next();
                }
            }); 
            
            app.UseWebApi(WebApiConfig.Configure(options));

            return app;
        }


    }
}
