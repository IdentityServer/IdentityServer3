using Microsoft.Owin.Hosting;
using Owin;
using Serilog;
using System.Diagnostics;

namespace Host.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var webApp = WebApp.Start("https://localhost:44333", app =>
            {
                app.UseIdentityServer();
            });
            
            System.Console.WriteLine("identityserver up and running....");
            Process.Start("https://localhost:44333/core");

            System.Console.ReadLine();
            webApp.Dispose();
        }
    }
}