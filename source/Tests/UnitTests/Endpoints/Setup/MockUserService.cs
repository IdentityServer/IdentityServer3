using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;

namespace IdentityServer3.Tests.Endpoints.Setup
{
    public class MockUserService : InMemoryUserService
    {
        public Func<PreAuthenticationContext, Task> OnPreAuthenticate { get; set; }
        public Func<LocalAuthenticationContext, Task> OnAuthenticateLocal { get; set; }
        public Func<ExternalAuthenticationContext, Task> OnAuthenticateExternal { get; set; }
        public Func<PostAuthenticationContext, Task> OnPostAuthenticate { get; set; }

        public bool AuthenticateExternalWasCalled { get; set; }
        public bool PostAuthenticateWasCalled { get; set; }
        public bool SignOutWasCalled { get; set; }

        public OwinEnvironmentService OwinEnvironmentService { get; set; }

        public MockUserService(List<InMemoryUser> users) : base(users)
        {
        }

        public async override Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            if (OnPreAuthenticate != null)
            {
                await OnPreAuthenticate(context);
            }
            else
            {
                await base.PreAuthenticateAsync(context);
            }
        }

        public async override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            if (OnAuthenticateLocal != null)
            {
                await OnAuthenticateLocal(context);
            }
            else
            {
                await base.AuthenticateLocalAsync(context);
            }
        }

        public async override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            AuthenticateExternalWasCalled = true;

            if (OnAuthenticateExternal != null)
            {
                await OnAuthenticateExternal(context);
            }
            else
            {
                await base.AuthenticateExternalAsync(context);
            }
        }

        public async override Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            PostAuthenticateWasCalled = true;
            if (OnPostAuthenticate != null)
            {
                await OnPostAuthenticate(context);
            }
            else
            {
                await base.PostAuthenticateAsync(context);
            }
        }

        public override Task SignOutAsync(SignOutContext context)
        {
            SignOutWasCalled = true;
            return base.SignOutAsync(context);
        }
    }
}
