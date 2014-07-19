using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;

namespace Owin
{
    static class UseEmbeddedFileServerExtension
    {
        public static IAppBuilder UseEmbeddedFileServer(this IAppBuilder app)
        {
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, "Thinktecture.IdentityServer.Core.Assets")
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets/libs/fonts"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, "Thinktecture.IdentityServer.Core.Assets.libs.bootstrap.fonts")
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            return app;
        }
    }
}
