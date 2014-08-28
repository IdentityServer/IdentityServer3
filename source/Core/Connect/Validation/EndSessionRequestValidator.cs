/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class EndSessionRequestValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ValidatedEndSessionRequest _validatedRequest;
        private readonly TokenValidator _tokenValidator;
        private readonly IClientStore _clients;

        public ValidatedEndSessionRequest ValidatedRequest
        {
            get { return _validatedRequest; }
        }

        public EndSessionRequestValidator(TokenValidator tokenValidator, IClientStore clients)
        {
            _tokenValidator = tokenValidator;
            _clients = clients;

            _validatedRequest = new ValidatedEndSessionRequest();
        }

        // basic protocol validation
        public async Task<ValidationResult> ValidateProtocol(NameValueCollection parameters)
        {
            Logger.Info("Start protocol validation");

            if (parameters == null)
            {
                Logger.Error("Parameters are null.");
                throw new ArgumentNullException("parameters");
            }

            _validatedRequest.Raw = parameters;

            //////////////////////////////////////////////////////////
            // id_token_hint is recommended, but not required
            /////////////////////////////////////////////////////////
            var idTokenHint = parameters.Get(Constants.EndSessionRequest.IdTokenHint);
            if (idTokenHint.IsPresent())
            {
                Logger.InfoFormat("id_token_hint: {0}", idTokenHint);
                _validatedRequest.IdTokenHint = idTokenHint;

                var tokenValidation = await _tokenValidator.ValidateIdentityTokenAsync(idTokenHint);
                if (tokenValidation.IsError)
                {
                    Logger.Info("Invalid id_token_hint");
                    return Invalid();
                }
                _validatedRequest.Claims = tokenValidation.Claims;
            }
            else
                Logger.Info("No id_token_hint supplied");

            //////////////////////////////////////////////////////////
            // post_logout_redirect_uri is optional
            //////////////////////////////////////////////////////////
            var postLogoutRedirectUri = parameters.Get(Constants.EndSessionRequest.PostLogoutRedirectUri);
            if (postLogoutRedirectUri.IsPresent())
            {
                Uri redirectUri;
                if (Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out redirectUri))
                {
                    Logger.InfoFormat("post_logout_redirect_uri: {0}", postLogoutRedirectUri);
                    _validatedRequest.PostLogoutRedirectUri = redirectUri;
                    var result = await ValidateRedirectUri(redirectUri, _validatedRequest.Claims);
                    if (result.IsError)
                        return result;
                }
            }
            else
                Logger.Info("No post_logout_redirect_uri supplied");

            //////////////////////////////////////////////////////////
            // state is optional
            //////////////////////////////////////////////////////////
            var state = parameters.Get(Constants.EndSessionRequest.State);
            if (state.IsPresent())
            {
                Logger.InfoFormat("State: {0}", state);
                _validatedRequest.State = state;
            }
            else
                Logger.Info("No state supplied");

            Logger.Info("Protocol validation successful");
            return Valid();
        }

        async Task<ValidationResult> ValidateRedirectUri(Uri postLogoutRedirectUri, IEnumerable<Claim> claims)
        {
            var clientId = claims == null ? null : claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Audience);
            if (clientId != null)
            {
                var client = await _clients.FindClientByIdAsync(clientId.Value);
                if (client == null || !client.Enabled)
                    return Invalid(error: Constants.EndSessionErrors.InvalidClient); // Unable to find valid client

                if (client.RedirectUris.Contains(postLogoutRedirectUri))
                    return Valid();
                else
                    return Invalid(error: Constants.EndSessionErrors.InvalidClient); // Invalid redirect uri
            }
            return Valid(); // Client not specified so we can't validate redirect uri
        }

        static ValidationResult Invalid(ErrorTypes errorType = ErrorTypes.User, string error = Constants.EndSessionErrors.InvalidRequest)
        {
            var result = new ValidationResult();

            result.IsError = true;
            result.Error = error;
            result.ErrorType = errorType;

            return result;
        }

        static ValidationResult Valid()
        {
            var result = new ValidationResult();
            result.IsError = false;

            return result;
        }
    }
}