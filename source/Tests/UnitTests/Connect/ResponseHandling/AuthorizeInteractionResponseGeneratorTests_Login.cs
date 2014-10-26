using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityModel;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Moq;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Models;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core;

namespace Thinktecture.IdentityServer.Tests.Connect.ResponseHandling
{
    [TestClass]
    public class AuthorizeInteractionResponseGeneratorTests_Login
    {
        [TestMethod]
        public async Task Anonymous_User_must_SignIn()
        {
            var generator = new AuthorizeInteractionResponseGenerator(null, null);

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo"
            };

            var result = await generator.ProcessLoginAsync(request, Principal.Anonymous);

            Assert.IsTrue(result.IsLogin);
        }

        [TestMethod]
        public async Task Authenticated_User_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            users.Setup(x => x.IsActiveAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<bool>(true));

            var generator = new AuthorizeInteractionResponseGenerator(null, users.Object);

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            Assert.IsFalse(result.IsLogin);
        }

        [TestMethod]
        public async Task Authenticated_User_with_allowed_current_Idp_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            users.Setup(x => x.IsActiveAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<bool>(true));

            var generator = new AuthorizeInteractionResponseGenerator(null, users.Object);

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client 
                {
                    IdentityProviderRestrictions = new List<string> 
                    {
                        Constants.BuiltInIdentityProvider
                    }
                }
            };

            var result = await generator.ProcessClientLoginAsync(request);

            Assert.IsFalse(result.IsLogin);
        }

        [TestMethod]
        public async Task Authenticated_User_with_restricted_current_Idp_must_SignIn()
        {
            var users = new Mock<IUserService>();
            users.Setup(x => x.IsActiveAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<bool>(true));

            var generator = new AuthorizeInteractionResponseGenerator(null, users.Object);

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    IdentityProviderRestrictions = new List<string> 
                    {
                        "some_idp"
                    }
                }
            };

            var result = await generator.ProcessClientLoginAsync(request);

            Assert.IsTrue(result.IsLogin);
        }

        [TestMethod]
        public async Task Authenticated_User_with_allowed_requested_Idp_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            users.Setup(x => x.IsActiveAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<bool>(true));

            var generator = new AuthorizeInteractionResponseGenerator(null, users.Object);

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                LoginHint = "idp:" + Constants.BuiltInIdentityProvider
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            Assert.IsFalse(result.IsLogin);
        }

        [TestMethod]
        public async Task Authenticated_User_with_different_requested_Idp_must_SignIn()
        {
            var users = new Mock<IUserService>();
            users.Setup(x => x.IsActiveAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<bool>(true));

            var generator = new AuthorizeInteractionResponseGenerator(null, users.Object);

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                LoginHint = "idp:some_idp"
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            Assert.IsTrue(result.IsLogin);
        }
    }
}
