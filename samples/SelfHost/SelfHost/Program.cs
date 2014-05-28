using Microsoft.Owin.Hosting;
using System;

namespace SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "IdentityServer v3 SelfHost";

            const string url = "http://localhost:3333/core";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server listening at {0}. Press a key to stop", url);
                Console.ReadKey();
            }
        }
    }
}