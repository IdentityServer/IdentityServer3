using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using Microsoft.Owin.Testing;
using Owin;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer3.Tests.Conformance
{
    public class IdentityServerHost : IDisposable
    {
        public string Url = "https://idsvr.test/";

        public List<Scope> Scopes = new List<Scope>();
        public List<Client> Clients = new List<Client>();
        public List<InMemoryUser> Users = new List<InMemoryUser>();

        public IdentityServerOptions Options;

        public TestServer Server;
        public HttpClient Client;
        
        public DateTimeOffset Now = DateTimeOffset.MinValue;

        public IdentityServerHost()
        {
            var clientStore = new InMemoryClientStore(Clients);
            var scopeStore = new InMemoryScopeStore(Scopes);
            var userService = new InMemoryUserService(Users);

            var factory = new IdentityServerServiceFactory
            {
                ScopeStore = new Registration<IScopeStore>(scopeStore),
                ClientStore = new Registration<IClientStore>(clientStore),
                UserService = new Registration<IUserService>(userService),
            };

            Options = new IdentityServerOptions
            {
                Factory = factory,
                DataProtector = new NoDataProtector(),
                SiteName = "IdentityServer3 Host",
                SigningCertificate = SigningCertificate
            };
        }

        public void Dispose()
        {
            Server.Dispose();
            DateTimeOffsetHelper.UtcNowFunc = () => DateTimeOffset.UtcNow;
        }

        public void Init()
        {
            DateTimeOffsetHelper.UtcNowFunc = () => UtcNow;
            
            Server = TestServer.Create(app =>
            {
                app.UseIdentityServer(Options);
            });
            
            NewRequest();
        }

        public HttpClient NewRequest()
        {
            return Client = Server.HttpClient;
        }

        public DateTimeOffset UtcNow
        {
            get
            {
                if (Now > DateTimeOffset.MinValue) return Now;
                return DateTimeOffset.UtcNow;
            }
        }

        X509Certificate2 SigningCertificate
        {
            get
            {
                var assembly = typeof(IdentityServerHost).Assembly;
                using (var stream = assembly.GetManifestResourceStream("IdentityServer3.Tests.idsrv3test.pfx"))
                {
                    return new X509Certificate2(ReadStream(stream), "idsrv3test");
                }
            }
        }

        private static byte[] ReadStream(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
