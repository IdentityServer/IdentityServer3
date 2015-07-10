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
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation
{
    public class CustomGrantValidation
    {
        const string Category = "Validation - Custom Grant Validation";

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Custom_Grant_Single_Validator()
        {
            var validator = new CustomGrantValidator(new[] { new TestGrantValidator() });
            var request = new ValidatedTokenRequest
            {
                GrantType = "custom_grant"
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeFalse();
            result.Principal.Should().NotBeNull();
            result.Principal.GetSubjectId().Should().Be("bob");
            result.Principal.GetAuthenticationMethod().Should().Be("CustomGrant");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Custom_Grant_Multiple_Validator()
        {
            var validator = new CustomGrantValidator(new List<ICustomGrantValidator> 
            { 
                new TestGrantValidator(), 
                new TestGrantValidator2() 
            });

            var request = new ValidatedTokenRequest
            {
                GrantType = "custom_grant"
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeFalse();
            result.Principal.Should().NotBeNull();
            result.Principal.GetSubjectId().Should().Be("bob");
            result.Principal.GetAuthenticationMethod().Should().Be("CustomGrant");

            request.GrantType = "custom_grant2";
            result = await validator.ValidateAsync(request);

            result.IsError.Should().BeFalse();
            result.Principal.Should().NotBeNull();
            result.Principal.GetSubjectId().Should().Be("alice");
            result.Principal.GetAuthenticationMethod().Should().Be("CustomGrant2");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Unknown_Custom_Grant_Multiple_Validator()
        {
            var validator = new CustomGrantValidator(new List<ICustomGrantValidator> 
            { 
                new TestGrantValidator(), 
                new TestGrantValidator2() 
            });

            var request = new ValidatedTokenRequest
            {
                GrantType = "unknown"
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Empty_Validator_List()
        {
            var validator = new CustomGrantValidator(new List<ICustomGrantValidator>());

            var request = new ValidatedTokenRequest
            {
                GrantType = "something"
            };

            var result = await validator.ValidateAsync(request);

            result.IsError.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetAvailable_Should_Return_Expected_GrantTypes()
        {
            var validator = new CustomGrantValidator(new List<ICustomGrantValidator> 
            { 
                new TestGrantValidator(), 
                new TestGrantValidator2() 
            });

            var available = validator.GetAvailableGrantTypes();

            available.Count().Should().Be(2);
            available.First().Should().Be("custom_grant");
            available.Skip(1).First().Should().Be("custom_grant2");
        }
    }
}