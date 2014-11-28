﻿/*
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

using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Connect.ResponseHandling
{
    
    public class AuthorizeInteractionResponseGeneratorTests_Consent
    {
        Mock<IConsentService> mockConsent;
        Mock<IUserService> mockUserService;
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
        private void AssertUpdateConsentCalled(Client client, ClaimsPrincipal user, params string[] scopes)
        {
            mockConsent.Verify(x => x.UpdateConsentAsync(client, user, scopes));
        }

        private void AssertErrorReturnsRequestValues(AuthorizeError error, ValidatedAuthorizeRequest request)
        {
            error.ResponseMode.Should().Be(request.ResponseMode);
            error.ErrorUri.Should().Be(request.RedirectUri);
            error.State.Should().Be(request.State);
        }
        
        public AuthorizeInteractionResponseGeneratorTests_Consent()
        {
            mockConsent = new Mock<IConsentService>();
            mockUserService = new Mock<IUserService>();
            subject = new AuthorizeInteractionResponseGenerator(mockConsent.Object, mockUserService.Object);
        }

        [Fact]
        public void ProcessConsentAsync_NullRequest_Throws()
        {
            Func<Task> act = () => subject.ProcessConsentAsync(null, new UserConsent());

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("request");
        }
        
        [Fact]
        public void ProcessConsentAsync_AllowsNullConsent()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.Consent
            }; 
            var result = subject.ProcessConsentAsync(request, null).Result;
        }

        [Fact]
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

            Func<Task> act = () => subject.ProcessConsentAsync(request);

            act.ShouldThrow<ArgumentException>()
                .And.Message.Should().Contain("PromptMode");
        }

        [Fact]
        public void ProcessConsentAsync_PromptModeIsSelectAccount_Throws()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.SelectAccount
            };

            Func<Task> act = () => subject.ProcessConsentAsync(request);

            act.ShouldThrow<ArgumentException>()
                .And.Message.Should().Contain("PromptMode");
        }


        [Fact]
        public void ProcessConsentAsync_RequiresConsentButPromptModeIsNone_ReturnsErrorResult()
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
            request.WasConsentShown.Should().BeFalse();
            result.IsError.Should().BeTrue();
            result.Error.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Error.Should().Be(Constants.AuthorizeErrors.InteractionRequired);
            AssertErrorReturnsRequestValues(result.Error, request);
            AssertUpdateConsentNotCalled();
        }
        
        [Fact]
        public void ProcessConsentAsync_PromptModeIsConsent_NoPriorConsent_ReturnsConsentResult()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.Consent
            };
            var result = subject.ProcessConsentAsync(request).Result;
            request.WasConsentShown.Should().BeFalse();
            result.IsConsent.Should().BeTrue();
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_NoPriorConsent_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.Consent
            };
            var result = subject.ProcessConsentAsync(request).Result;
            request.WasConsentShown.Should().BeFalse();
            result.IsConsent.Should().BeTrue();
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_PromptModeIsConsent_ConsentNotGranted_ReturnsErrorResult()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.Consent
            };
            var consent = new UserConsent
            {
                Button = "no",
                RememberConsent = false,
                Scopes = new string[] { "read", "write" }
            };
            var result = subject.ProcessConsentAsync(request, consent).Result;
            request.WasConsentShown.Should().BeTrue();
            result.IsError.Should().BeTrue();
            result.Error.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Error.Should().Be(Constants.AuthorizeErrors.AccessDenied);
            AssertErrorReturnsRequestValues(result.Error, request);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentNotGranted_ReturnsErrorResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
            };
            var consent = new UserConsent
            {
                Button = "no",
                RememberConsent = false,
                Scopes = new string[] { "read", "write" }
            };
            var result = subject.ProcessConsentAsync(request, consent).Result;
            request.WasConsentShown.Should().BeTrue();
            result.IsError.Should().BeTrue();
            result.Error.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Error.Should().Be(Constants.AuthorizeErrors.AccessDenied);
            AssertErrorReturnsRequestValues(result.Error, request);
            AssertUpdateConsentNotCalled();
        }



        [Fact]
        public void ProcessConsentAsync_PromptModeIsConsent_ConsentGranted_NoScopesSelected_ReturnsConsentResult()
        {
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                PromptMode = Constants.PromptModes.Consent,
                ValidatedScopes = new ScopeValidator(),
                Client = new Client { }
            };
            var consent = new UserConsent
            {
                Button = "yes",
                RememberConsent = false,
                Scopes = new string[] {  }
            };
            var result = subject.ProcessConsentAsync(request, consent).Result;
            request.WasConsentShown.Should().BeTrue();
            result.IsConsent.Should().BeTrue();
            result.ConsentError.Should().Be(Messages.MustSelectAtLeastOnePermission);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentGranted_NoScopesSelected_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                ValidatedScopes = new ScopeValidator(),
                Client = new Client { }
            };
            var consent = new UserConsent
            {
                Button = "yes",
                RememberConsent = false,
                Scopes = new string[] {  }
            };
            var result = subject.ProcessConsentAsync(request, consent).Result;
            request.WasConsentShown.Should().BeTrue();
            result.IsConsent.Should().BeTrue();
            result.ConsentError.Should().Be(Messages.MustSelectAtLeastOnePermission);
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_NoPromptMode_ConsentServiceRequiresConsent_ConsentGranted_ScopesSelected_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                ValidatedScopes = new ScopeValidator(),
                Client = new Client { }
            };
            request.ValidatedScopes.AreScopesValid(new string[] { "read", "write" }, TestScopes.Get());
            var consent = new UserConsent
            {
                Button = "yes",
                RememberConsent = false,
                Scopes = new string[] { "read" }
            };
            var result = subject.ProcessConsentAsync(request, consent).Result;
            request.ValidatedScopes.GrantedScopes.Count.Should().Be(1);
            "read".Should().Be(request.ValidatedScopes.GrantedScopes.First().Name);
            request.WasConsentShown.Should().BeTrue();
            result.IsConsent.Should().BeFalse();
            AssertUpdateConsentNotCalled();
        }
        
        [Fact]
        public void ProcessConsentAsync_PromptModeConsent_ConsentGranted_ScopesSelected_ReturnsConsentResult()
        {
            RequiresConsent(true);
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                ValidatedScopes = new ScopeValidator(),
                Client = new Client { }
            };
            request.ValidatedScopes.AreScopesValid(new string[] { "read", "write" }, TestScopes.Get());
            var consent = new UserConsent
            {
                Button = "yes",
                RememberConsent = false,
                Scopes = new string[] { "read" }
            };
            var result = subject.ProcessConsentAsync(request, consent).Result;
            request.ValidatedScopes.GrantedScopes.Count.Should().Be(1);
            "read".Should().Be(request.ValidatedScopes.GrantedScopes.First().Name);
            request.WasConsentShown.Should().BeTrue();
            result.IsConsent.Should().BeFalse();
            AssertUpdateConsentNotCalled();
        }

        [Fact]
        public void ProcessConsentAsync_AllowConsentSelected_SavesConsent()
        {
            RequiresConsent(true);
            var client = new Client { AllowRememberConsent = true };
            var user = new ClaimsPrincipal();
            var request = new ValidatedAuthorizeRequest()
            {
                ResponseMode = Constants.ResponseModes.Fragment,
                State = "12345",
                RedirectUri = new Uri("https://client.com/callback"),
                ValidatedScopes = new ScopeValidator(),
                Client = client,
                Subject = user
            };
            request.ValidatedScopes.AreScopesValid(new string[] { "read", "write" }, TestScopes.Get());
            var consent = new UserConsent
            {
                Button = "yes",
                RememberConsent = true,
                Scopes = new string[] { "read" }
            };
            var result = subject.ProcessConsentAsync(request, consent).Result;
            AssertUpdateConsentCalled(client, user, "read");
        }

    }
}
