using System;
using Microsoft.Owin.Hosting;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.TestServices;

namespace SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string url = "http://localhost:3333/core";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server listening at {0}. Press a key to stop", url);
                Console.ReadKey();
            }
        }

        internal class Startup
        {
            public void Configuration(IAppBuilder appBuilder)
            {
                IdentityServerServiceFactory factory = TestOptionsFactory.Create("https://idsrv3.com",
                    "Thinktecture IdentityServer v3", "http://localhost:3333");

                var opts = new IdentityServerCoreOptions
                {
                    Factory = factory,
                };

                appBuilder.UseIdentityServerCore(opts);
            }
        }
    }
}