using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin;
using System.Net.Http;
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
        protected IdentityServerOptions options;

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
                var factory = TestFactory.Create();

                mockUserService = new Mock<InMemoryUserService>(TestUsers.Get());
                mockUserService.CallBase = true;
                factory.UserService = Registration.RegisterFactory<IUserService>(() => mockUserService.Object);

                options = TestIdentityServerOptions.Create();
                options.Factory = factory;
                options.AdditionalIdentityProviderConfiguration = ConfigureAdditionalIdentityProviders;
                
                protector = options.DataProtector;
                
                app.UseIdentityServer(options);
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
            if (path.StartsWith("/")) path = path.Substring(1);
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
