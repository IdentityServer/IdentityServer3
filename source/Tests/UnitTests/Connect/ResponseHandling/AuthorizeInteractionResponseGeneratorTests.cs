using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Moq;
using Thinktecture.IdentityServer.Core.Connect;

namespace Thinktecture.IdentityServer.Tests.Connect.ResponseHandling
{
    [TestClass]
    public class AuthorizeInteractionResponseGeneratorTests
    {
        Mock<IConsentService> consent;
        AuthorizeInteractionResponseGenerator subject;

        [TestInitialize]
        public void Init()
        {
            consent = new Mock<IConsentService>();
            subject = new AuthorizeInteractionResponseGenerator(consent.Object);
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

    }
}
