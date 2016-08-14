using System;
using System.IO;
using System.Text;

namespace Owin
{
    internal static class ConfigureRequestBodyBufferExtension
    {
        public static IAppBuilder ConfigureRequestBodyBuffer(this IAppBuilder app)
        {
            if (app == null) throw new ArgumentNullException("app");

            app.Use(async (context, next) =>
            {
                var originalBodyStream = context.Request.Body;
                using (var bodyReader = new StreamReader(originalBodyStream))
                {
                    var body = bodyReader.ReadToEnd();
                    var requestData = Encoding.UTF8.GetBytes(body);
                    using (var ms = new MemoryStream(requestData))
                    {
                        context.Request.Body = ms;

                        try
                        {
                            await next();
                        }
                        finally
                        {
                            context.Request.Body = originalBodyStream;
                        }
                    }
                }
            });

            return app;
        }
    }
}