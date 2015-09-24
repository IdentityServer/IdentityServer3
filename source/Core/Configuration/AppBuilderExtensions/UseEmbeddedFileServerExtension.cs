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

using IdentityServer3.Core;
using IdentityServer3.Core.Services.Default;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

namespace Owin
{
    internal static class UseEmbeddedFileServerExtension
    {
        public static IAppBuilder UseEmbeddedFileServer(this IAppBuilder app)
        {
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, AssetManager.HttpAssetsNamespace)
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets/libs/fonts"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(Constants).Assembly, AssetManager.FontAssetsNamespace)
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            return app;
        }
    }
}
