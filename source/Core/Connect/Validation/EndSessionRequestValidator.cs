using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Text;
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

            var idTokenHint = parameters.Get(Constants.EndSessionRequest.IdTokenHint);
            if (idTokenHint.IsPresent())
            {
                // validate id_token
                var tokenValidationResult = await _tokenValidator.ValidateIdentityTokenAsync(idTokenHint);
                if (tokenValidationResult.IsError)
                {
                    return Invalid();
                }

                // get client_id
                var clientIdClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Audience);
                if (clientIdClaim == null)
                {
                    return Invalid();
                }

                // get client
                var client = await _clients.FindClientByIdAsync(clientIdClaim.Value);
                if (client == null)
                {
                    return Invalid();
                }

                _validatedRequest.Client = client;

                // validate sub claim against currently logged on user
                var subClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
                if (subClaim != null)
                {
                    if (subject.GetSubjectId() != subClaim.Value)
                    {
                        return Invalid();
                    }
                }
            }

            var redirectUrl = parameters.Get(Constants.EndSessionRequest.PostLogoutRedirectUri);
            return Invalid();
        }

        private ValidationResult Invalid()
        {
            throw new NotImplementedException();
        }
    }
}
