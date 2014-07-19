using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Moq;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace Thinktecture.IdentityServer.Tests.Connect.ResponseHandling
{
    [TestClass]
    public class AuthorizeInteractionResponseGeneratorTests
    {
        Mock<IConsentService> mockConsent;
        AuthorizeInteractionResponseGenerator subject;

        void RequiresConsent(bool value)
        {
            mockConsent.Setup(x => x.RequiresConsentAsync(It.IsAny<Client>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<IEnumerable<string>>()))
               .Returns(Task.FromResult(value));
        }
        
        private void AssertUpdateConsentNotCalled()
        {
            mockConsent.Verify(x => x.UpdateConsentAsync(It.IsAny<Client>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<IEnumerable<string>>()), Times.Never());
        }

        private void AssertErrorReturnsRequestValues(AuthorizeError error, ValidatedAuthorizeRequest request)
        {
            Assert.AreEqual(request.ResponseMode, error.ResponseMode);
            Assert.AreEqual(request.RedirectUri, error.ErrorUri);
            Assert.AreEqual(request.State, error.State);
        }

        [TestInitialize]
        public void Init()
        {
            mockConsent = new Mock<IConsentService>();
            subject = new AuthorizeInteractionResponseGenerator(mockConsent.Object);
        }

        [TestMethod]
        public void ProcessConsentAsync_NullRequest_Throws()
        {
            try
            {
                var result = subject.ProcessConsentAsync(null, new Core.Connect.Models.UserConsent()).Result;
                Assert.Fail();
            }
            catch(AggregateException ex){
                ArgumentNullException ex2 = (ArgumentNullException)ex.InnerException;
                StringAssert.Contains(ex2.ParamName, "request");
            }
        }
        
        [TestMethod]
        public void ProcessConsentAsync_AllowsNullConsent()
        {
            var result = subject.ProcessConsentAsync(new ValidatedAuthorizeRequest(), null).Result;
        }

        [TestMethod]
        public void ProcessConsentAsync_PromptModeIsLogin_Throws()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.Login
            };

            try
            {
                var result = subject.ProcessConsentAsync(request).Result;
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                StringAssert.Contains(ex.Message, "PromptMode");
            }
        }
        //[TestMethod]
        //public void ProcessConsentAsync_PromptModeIsLogin_Throws()
        //{
        //    RequiresConsent(true);
        //    var request = new ValidatedAuthorizeRequest()
        //    {
        //        ResponseMode = Constants.ResponseModes.Fragment,
        //        State = "12345",
        //        RedirectUri = new Uri("https://client.com/callback"),
        //        PromptMode = Constants.PromptModes.Login
        //    };

        //    try
        //    {
        //        var result = subject.ProcessConsentAsync(request).Result;
        //        Assert.Fail();
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        StringAssert.Contains(ex.Message, "PromptMode");
        //    }
        //}

        [TestMethod]
        public void ProcessConsentAsync_RequiresConsentButPromptModeIsNone_ReturnsError()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.None
            };
            var result = subject.ProcessConsentAsync(request).Result;
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ErrorTypes.Client, result.Error.ErrorType);
            Assert.AreEqual(Constants.AuthorizeErrors.InteractionRequired, result.Error.Error);
        }

        [TestMethod]
        public void ProcessConsentAsync_RequiresConsentButPromptModeIsNone_ErrorReturnsRequestValues()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.None
            };
            var result = subject.ProcessConsentAsync(request).Result;
            AssertErrorReturnsRequestValues(result.Error, request);
        }
        
        [TestMethod]
        public void ProcessConsentAsync_RequiresConsentButPromptModeIsNone_DoesNotCallUpdateConsent()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.None
            };
            var result = subject.ProcessConsentAsync(request).Result;
            AssertUpdateConsentNotCalled();
        }


    }
}
