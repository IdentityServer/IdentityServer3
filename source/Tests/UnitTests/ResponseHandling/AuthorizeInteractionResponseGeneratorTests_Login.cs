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

using FluentAssertions;
using IdentityModel;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.ResponseHandling;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Validation;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Connect.ResponseHandling
{
    
    public class AuthorizeInteractionResponseGeneratorTests_Login
    {
        IdentityServerOptions options;

        public AuthorizeInteractionResponseGeneratorTests_Login()
        {
            options = new IdentityServerOptions();
        }

        [Fact]
        public async Task Anonymous_User_must_SignIn()
        {
            var generator = new AuthorizeInteractionResponseGenerator(options, null, null, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo"
            };

            var result = await generator.ProcessLoginAsync(request, Principal.Anonymous);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client()
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_allowed_current_Idp_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

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

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_restricted_current_Idp_must_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

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

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_allowed_requested_Idp_must_not_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client(),
                 AuthenticationContextReferenceClasses = new List<string>{
                    "idp:" + Constants.BuiltInIdentityProvider
                }
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            result.IsLogin.Should().BeFalse();
        }

        [Fact]
        public async Task Authenticated_User_with_different_requested_Idp_must_SignIn()
        {
            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Client = new Client(),
                AuthenticationContextReferenceClasses = new List<string>{
                    "idp:some_idp"
                },
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessLoginAsync(request, principal);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_local_Idp_must_SignIn_when_global_options_does_not_allow_local_logins()
        {
            options.AuthenticationOptions.EnableLocalLogin = false;

            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    ClientId = "foo",
                    EnableLocalLogin = true
                }
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessClientLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }

        [Fact]
        public async Task Authenticated_User_with_local_Idp_must_SignIn_when_client_options_does_not_allow_local_logins()
        {
            options.AuthenticationOptions.EnableLocalLogin = true;

            var users = new Mock<IUserService>();
            var generator = new AuthorizeInteractionResponseGenerator(options, null, users.Object, new DefaultLocalizationService());

            var request = new ValidatedAuthorizeRequest
            {
                ClientId = "foo",
                Subject = IdentityServerPrincipal.Create("123", "dom"),
                Client = new Client
                {
                    ClientId = "foo",
                    EnableLocalLogin = false
                }
            };

            var principal = IdentityServerPrincipal.Create("123", "dom");
            var result = await generator.ProcessClientLoginAsync(request);

            result.IsLogin.Should().BeTrue();
        }
    }
}
