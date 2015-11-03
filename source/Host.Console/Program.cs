/*
 * Copyright 2014 Dominick Baier, Brock Allen
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

using Microsoft.Owin.Hosting;
using Owin;
using Serilog;
using System;
using System.Diagnostics;

namespace Host.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.Title = "IdentityServer3";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var webApp = WebApp.Start("https://localhost:44333", app =>
            {
                app.UseIdentityServer();
            });
            
            System.Console.WriteLine("identityserver up and running....");
            
            while(true)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.B)
                {
                    Process.Start("https://localhost:44333/core");
                }
                else
                {
                    break;
                }
            }
            
            webApp.Dispose();
        }
    }
}