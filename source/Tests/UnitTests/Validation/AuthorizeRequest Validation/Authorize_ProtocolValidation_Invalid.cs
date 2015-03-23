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
using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Tests.Validation.Setup;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Validation.AuthorizeRequest_Validation
{
    public class Authorize_ProtocolValidation_Invalid
    {
        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public void Null_Parameter()
        {
            var validator = Factory.CreateAuthorizeRequestValidator();

            Func<Task> act = () => validator.ValidateAsync(null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Empty_Parameters()
        {
            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(new NameValueCollection());

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        // fails because openid scope is requested, but no response type that indicates an identity token
        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task OpenId_Token_Only_Request()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "implicitclient"},
                {Constants.AuthorizeRequest.SCOPE, Constants.StandardScopes.OPEN_ID},
                {Constants.AuthorizeRequest.REDIRECT_URI, "oob://implicit/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.TOKEN}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_SCOPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Resource_Only_IdToken_Request()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "implicitclient"},
                {Constants.AuthorizeRequest.SCOPE, "resource"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "oob://implicit/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.ID_TOKEN},
                {Constants.AuthorizeRequest.NONCE, "abc"}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Mixed_Token_Only_Request()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "implicitclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid resource"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "oob://implicit/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.TOKEN}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_SCOPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task OpenId_IdToken_Request_Nonce_Missing()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "implicitclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "oob://implicit/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.ID_TOKEN}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Missing_ClientId()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/callback"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Missing_Scope()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Missing_RedirectUri()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "client"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Malformed_RedirectUri()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "client"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "malformed"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Malformed_RedirectUri_Triple_Slash()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "client"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https:///attacker.com"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Missing_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Unknown_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, "unknown"}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Invalid_ResponseMode_For_Code_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE},
                {Constants.AuthorizeRequest.RESPONSE_MODE, Constants.ResponseModes.FRAGMENT}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Invalid_ResponseMode_For_IdToken_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "implicitclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "oob://implicit/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.ID_TOKEN},
                {Constants.AuthorizeRequest.RESPONSE_MODE, Constants.ResponseModes.QUERY}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Invalid_ResponseMode_For_IdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "implicitclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "oob://implicit/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.ID_TOKEN_TOKEN},
                {Constants.AuthorizeRequest.RESPONSE_MODE, Constants.ResponseModes.QUERY}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Invalid_ResponseMode_For_CodeToken_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "hybridclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE_TOKEN},
                {Constants.AuthorizeRequest.RESPONSE_MODE, Constants.ResponseModes.QUERY}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Invalid_ResponseMode_For_CodeIdToken_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "hybridclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE_ID_TOKEN},
                {Constants.AuthorizeRequest.RESPONSE_MODE, Constants.ResponseModes.QUERY}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Invalid_ResponseMode_For_CodeIdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "hybridclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE_ID_TOKEN_TOKEN},
                {Constants.AuthorizeRequest.RESPONSE_MODE, Constants.ResponseModes.QUERY}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.USER);
            result.Error.Should().Be(Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Malformed_MaxAge()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE},
                {Constants.AuthorizeRequest.MAX_AGE, "malformed"}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Protocol Validation")]
        public async Task Negative_MaxAge()
        {
            var parameters = new NameValueCollection {
                {Constants.AuthorizeRequest.CLIENT_ID, "codeclient"},
                {Constants.AuthorizeRequest.SCOPE, "openid"},
                {Constants.AuthorizeRequest.REDIRECT_URI, "https://server/cb"},
                {Constants.AuthorizeRequest.RESPONSE_TYPE, Constants.ResponseTypes.CODE},
                {Constants.AuthorizeRequest.MAX_AGE, "-1"}
            };

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.CLIENT);
            result.Error.Should().Be(Constants.AuthorizeErrors.INVALID_REQUEST);
        }
    }
}