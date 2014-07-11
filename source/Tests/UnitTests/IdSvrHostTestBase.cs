using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Tests
{
    public class IdSvrHostTestBase
    {
        protected TestServer server;
        protected HttpClient client;
        protected Thinktecture.IdentityServer.Core.Configuration.IDataProtector protector;

        protected Mock<InMemoryUserService> mockUserService;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());
            // white space between unit tests
            LogProvider.GetLogger("").Debug("----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            LogProvider.GetLogger("").Debug("UNIT TEST: " + TestContext.TestName);
            LogProvider.GetLogger("").Debug("----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            
            server = TestServer.Create(app =>
            {

                protector = new NoDataProtector();

                var factory = TestFactory.Create(
                         issuerUri: "https://idsrv3.com",
                         siteName: "Thinktecture IdentityServer v3 - test",
                         publicHostAddress: "http://localhost:3333");

                mockUserService = new Mock<InMemoryUserService>(TestUsers.Get());
                mockUserService.CallBase = true;
                factory.UserService = Registration.RegisterFactory<IUserService>(() => mockUserService.Object);

                var opts = new IdentityServerOptions
                {
                    DataProtector = protector,
                    Factory = factory,
                    AdditionalIdentityProviderConfiguration = ConfigureAdditionalIdentityProviders
                };

                app.UseIdentityServer(opts);
            });
            client = server.HttpClient;
        }

        public virtual void ConfigureAdditionalIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.Use(async (ctx, next) =>
            {
                Preprocess(ctx);
                await next();
                Postprocess(ctx);
            });

            var google = new Microsoft.Owin.Security.Google.GoogleAuthenticationOptions
            {
                AuthenticationType = "Google",
                SignInAsAuthenticationType = signInAsType
            };
            app.UseGoogleAuthentication(google);
        }

        protected virtual void Preprocess(IOwinContext ctx)
        {
        }
        protected virtual void Postprocess(IOwinContext ctx)
        {
        }

        protected string Url(string path)
        {
            return "https://localhost:3333/" + path;
        }
        protected HttpResponseMessage Get(string path)
        {
            return client.GetAsync(Url(path)).Result;
        }

        protected T Get<T>(string path)
        {
            var result = Get(path);
            Assert.IsTrue(result.IsSuccessStatusCode);
            return result.Content.ReadAsAsync<T>().Result;
        }

        protected HttpResponseMessage Post<T>(string path, T value)
        {
            return client.PostAsJsonAsync(Url(path), value).Result;
        }

        protected HttpResponseMessage Put<T>(string path, T value)
        {
            return client.PutAsJsonAsync(Url(path), value).Result;
        }

        protected HttpResponseMessage Delete(string path)
        {
            return client.DeleteAsync(Url(path)).Result;
        }
    }
}
