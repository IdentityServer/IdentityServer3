using Microsoft.Owin;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class EndSessionRequestValidator
    {
        private readonly ValidatedEndSessionRequest _validatedRequest;
        private readonly TokenValidator _tokenValidator;
        private readonly IClientStore _clients;

        public ValidatedEndSessionRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public EndSessionRequestValidator(IdentityServerOptions options, IOwinContext context, TokenValidator tokenValidator, IClientStore clients)
        {
            _tokenValidator = tokenValidator;
            _clients = clients;

            _validatedRequest = new ValidatedEndSessionRequest
            {
                Options = options,
                Environment = context.Environment
            };
        }

        public async Task<ValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            _validatedRequest.Raw = parameters;
            _validatedRequest.Subject = subject;

            if (!subject.Identity.IsAuthenticated)
            {
                return Invalid();
            }

            var idTokenHint = parameters.Get(Constants.EndSessionRequest.IdTokenHint);
            if (idTokenHint.IsPresent())
            {
                // validate id_token
                var tokenValidationResult = await _tokenValidator.ValidateIdentityTokenAsync(idTokenHint);
                if (tokenValidationResult.IsError)
                {
                    return Invalid();
                }

                _validatedRequest.Client = tokenValidationResult.Client;

                // validate sub claim against currently logged on user
                var subClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
                if (subClaim != null)
                {
                    if (subject.GetSubjectId() != subClaim.Value)
                    {
                        return Invalid();
                    }
                }

                var redirectUri = parameters.Get(Constants.EndSessionRequest.PostLogoutRedirectUri);
                if (redirectUri.IsPresent())
                {
                    Uri uri;
                    if (Uri.TryCreate(redirectUri, UriKind.Absolute, out uri))
                    {
                        if (_validatedRequest.Client.PostLogoutRedirectUris.Contains(uri))
                        {
                            _validatedRequest.PostLogOutUri = uri;
                        }
                        else
                        {
                            return Invalid();
                        }
                    }
                    
                    var state = parameters.Get(Constants.EndSessionRequest.State);
                    if (state.IsPresent())
                    {
                        _validatedRequest.State = state;
                    }
                }
            }
            
            return Valid();
        }

        private ValidationResult Valid()
        {
            return new ValidationResult
            {
                IsError = false
            };
        }

        private ValidationResult Invalid()
        {
            return new ValidationResult
            {
                IsError = true,
                Error = "Invalid request"
            };
        }
    }
}