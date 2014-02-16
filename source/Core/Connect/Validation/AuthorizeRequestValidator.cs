using System;
using System.Collections.Specialized;
using System.Linq;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class AuthorizeRequestValidator
    {
        ILogger _logger;

        ValidatedAuthorizeRequest _validatedRequest;
        private ICoreSettings _core;

        public ValidatedAuthorizeRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public AuthorizeRequestValidator(ICoreSettings core, ILogger logger)
        {
            _core = core;
            _logger = logger;

            _validatedRequest = new ValidatedAuthorizeRequest();
            _validatedRequest.CoreSettings = _core;
        }

        // basic protocol validation
        public ValidationResult ValidateProtocol(NameValueCollection parameters)
        {
            _logger.Verbose("OIDC authorize request protocol validation");

            if (parameters == null)
            {
                _logger.Error("Parameters are null.");
                throw new ArgumentNullException("parameters");
            }

            //////////////////////////////////////////////////////////
            // client_id must be present
            /////////////////////////////////////////////////////////
            var clientId = parameters.Get(Constants.AuthorizeRequest.ClientId);
            if (clientId.IsMissing())
            {
                _logger.Error("client_id is missing");
                return Invalid();
            }

            _logger.InformationFormat("client_id: {0}", clientId);
            _validatedRequest.ClientId = clientId;

            //////////////////////////////////////////////////////////
            // redirect_uri must be present, and a valid uri
            //////////////////////////////////////////////////////////
            var redirectUri = parameters.Get(Constants.AuthorizeRequest.RedirectUri);

            if (redirectUri.IsMissing())
            {
                _logger.Error("redirect_uri is missing");
                return Invalid();
            }

            Uri uri;
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out uri))
            {
                _logger.ErrorFormat("invalid redirect_uri: {0}", redirectUri);
                return Invalid();
            }

            _logger.InformationFormat("redirect_uri: {0}", redirectUri);
            _validatedRequest.RedirectUri = new Uri(redirectUri);

            //////////////////////////////////////////////////////////
            // response_type must be present and supported
            //////////////////////////////////////////////////////////
            var responseType = parameters.Get(Constants.AuthorizeRequest.ResponseType);
            if (responseType.IsMissing())
            {
                _logger.Error("Missing response_type");
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
            }

            if (!Constants.SupportedResponseTypes.Contains(responseType))
            {
                _logger.ErrorFormat("Response type not supported: {0}", responseType);
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
            }

            _logger.InformationFormat("response_type: {0}", responseType);
            _validatedRequest.ResponseType = responseType;

            if (_validatedRequest.ResponseType == Constants.ResponseTypes.Code)
            {
                _logger.Information("Flow: code");
                _validatedRequest.Flow = Flows.Code;
                _validatedRequest.ResponseMode = Constants.ResponseModes.Query;
            }
            else
            {
                _logger.Information("Flow: implicit");
                _validatedRequest.Flow = Flows.Implicit;
                _validatedRequest.ResponseMode = Constants.ResponseModes.Fragment;
            }

            //////////////////////////////////////////////////////////
            // scope must be present
            //////////////////////////////////////////////////////////
            var scope = parameters.Get(Constants.AuthorizeRequest.Scope);
            if (scope.IsMissing())
            {
                _logger.Error("scope is missing");
                return Invalid(ErrorTypes.Client);
            }

            scope = scope.Trim();

            if (scope.Contains(Constants.StandardScopes.OpenId))
            {
                _validatedRequest.IsOpenIdRequest = true;
            }

            _validatedRequest.Scopes = scope.Split(' ').Distinct().ToList();
            _logger.InformationFormat("scopes: {0}", scope);

            //////////////////////////////////////////////////////////
            // check scope vs response type plausability
            //////////////////////////////////////////////////////////

            // if response_type is code - all scope variations are allowed
            if (_validatedRequest.ResponseType != Constants.ResponseTypes.Code)
            {
                // openid requests require a requested id_token
                if (_validatedRequest.IsOpenIdRequest)
                {
                    if (!_validatedRequest.ResponseType.Contains(Constants.ResponseTypes.IdToken))
                    {
                        _logger.Error("Request contains openid scope, but response_type does not contain an identity token");
                        return Invalid(ErrorTypes.Client);
                    }
                }
                else
                {
                    // resource requests require a token response_type
                    if (_validatedRequest.ResponseType != Constants.ResponseTypes.Token)
                    {
                        _logger.Error("Request does not contain the openid scope, but response_type contains an identity token");
                        return Invalid(ErrorTypes.Client);
                    }
                }
            }

            //////////////////////////////////////////////////////////
            // check state
            //////////////////////////////////////////////////////////
            var state = parameters.Get(Constants.AuthorizeRequest.State);
            if (state.IsPresent())
            {
                _logger.InformationFormat("State: {0}", state);
                _validatedRequest.State = state;
            }
            else
            {
                _logger.Information("No state supplied");
            }

            //////////////////////////////////////////////////////////
            // check nonce
            //////////////////////////////////////////////////////////
            var nonce = parameters.Get(Constants.AuthorizeRequest.Nonce);
            if (nonce.IsPresent())
            {
                _logger.InformationFormat("Nonce: {0}", nonce);
                _validatedRequest.Nonce = nonce;
            }
            else
            {
                _logger.Information("No nonce supplied");

                if (_validatedRequest.Flow == Flows.Implicit)
                {
                    // only openid requests require nonce
                    if (_validatedRequest.IsOpenIdRequest)
                    {
                        _logger.Error("Nonce required for implicit flow with openid scope");
                        return Invalid(ErrorTypes.Client);
                    }
                }
            }

            //////////////////////////////////////////////////////////
            // check response_mode
            //////////////////////////////////////////////////////////
            var responseMode = parameters.Get(Constants.AuthorizeRequest.ResponseMode);
            if (responseMode.IsPresent())
            {
                if (Constants.SupportedResponseModes.Contains(responseMode))
                {
                    if (responseMode == Constants.ResponseModes.FormPost &&
                        _validatedRequest.ResponseType != Constants.ResponseTypes.IdToken)
                    {
                        _logger.Error("Invalid response_type for response_mode");
                        return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
                    }

                    _validatedRequest.ResponseMode = responseMode;
                    _logger.InformationFormat("response_mode: {0}", responseMode);
                }
                else
                {
                    _logger.Information("Unsupported response_mode - ignored.");
                }
            }

            //////////////////////////////////////////////////////////
            // check prompt
            //////////////////////////////////////////////////////////
            var prompt = parameters.Get(Constants.AuthorizeRequest.Prompt);
            if (prompt.IsPresent())
            {
                _logger.InformationFormat("prompt: {0}", prompt);

                if (Constants.SupportedPromptModes.Contains(prompt))
                {
                    _validatedRequest.PromptMode = prompt;
                }
                else
                {
                    _logger.Information("Unsupported prompt mode - ignored.");
                }
            }

            //////////////////////////////////////////////////////////
            // check ui locales
            //////////////////////////////////////////////////////////
            var uilocales = parameters.Get(Constants.AuthorizeRequest.UiLocales);
            if (uilocales.IsPresent())
            {
                _validatedRequest.UiLocales = uilocales;
            }

            //////////////////////////////////////////////////////////
            // check max_age
            //////////////////////////////////////////////////////////
            var maxAge = parameters.Get(Constants.AuthorizeRequest.MaxAge);
            if (maxAge.IsPresent())
            {
                _logger.InformationFormat("max_age: {0}", maxAge);

                int seconds;
                if (int.TryParse(maxAge, out seconds))
                {
                    if (seconds >= 0)
                    {
                        _validatedRequest.MaxAge = seconds;
                    }
                    else
                    {
                        _logger.Error("Invalid max_age.");
                        return Invalid(ErrorTypes.Client);
                    }
                }
                else
                {
                    _logger.Error("Invalid max_age.");
                    return Invalid(ErrorTypes.Client);
                }
            }

            // todo: parse amr, acr

            return Valid();
        }

        public ValidationResult ValidateClient()
        {
            if (_validatedRequest.ClientId.IsMissing())
            {
                throw new InvalidOperationException("ClientId is empty. Validate protocol first.");
            }

            //////////////////////////////////////////////////////////
            // check for valid client
            //////////////////////////////////////////////////////////
            var client = _core.FindClientById(_validatedRequest.ClientId);
            if (client == null)
            {
                _logger.ErrorFormat("Unknown client: {0}", _validatedRequest.ClientId);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            _logger.InformationFormat("Client found in registry: {0} / {1}", client.ClientId, client.ClientName);
            _validatedRequest.Client = client;

            //////////////////////////////////////////////////////////
            // check if redirect_uri is valid
            //////////////////////////////////////////////////////////
            if (!_validatedRequest.Client.RedirectUris.Contains(_validatedRequest.RedirectUri))
            {
                _logger.ErrorFormat("Invalid redirect_uri: {0}", _validatedRequest.RedirectUri);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            //////////////////////////////////////////////////////////
            // check if flow is allowed for client
            //////////////////////////////////////////////////////////
            if (_validatedRequest.Flow != _validatedRequest.Client.Flow)
            {
                _logger.ErrorFormat("Invalid flow for client: {0}", _validatedRequest.Flow);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            //////////////////////////////////////////////////////////
            // check scopes and scope restrictions
            //////////////////////////////////////////////////////////
            if (_validatedRequest.Client.ScopeRestrictions == null ||
                _validatedRequest.Client.ScopeRestrictions.Count == 0)
            {
                _logger.Information("All scopes allowed for client");
            }
            else
            {
                _logger.Information("Allowed scopes for client client: " + _validatedRequest.Client.ScopeRestrictions.ToSpaceSeparatedString());

                foreach (var scope in _validatedRequest.Scopes)
                {
                    if (!_validatedRequest.Client.ScopeRestrictions.Contains(scope))
                    {
                        _logger.ErrorFormat("Requested scope not allowed: {0}", scope);
                        return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
                    }
                }
            }

            //////////////////////////////////////////////////////////
            // check if scopes are valid/supported and check for resource scopes
            //////////////////////////////////////////////////////////
            var scopeDetails = _core.GetScopes();
            foreach (var scope in _validatedRequest.Scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);

                if (scopeDetail == null)
                {
                    _logger.ErrorFormat("Invalid scope: {0}", scope);
                    return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
                }

                if (scopeDetail.IsOpenIdScope && !_validatedRequest.IsOpenIdRequest)
                {
                    _logger.ErrorFormat("Identity related scope requests, but no openid scope: {0}", scope);
                    return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
                }

                if (!scopeDetail.IsOpenIdScope)
                {
                    _validatedRequest.IsResourceRequest = true;
                }
            }

            //////////////////////////////////////////////////////////
            // check id vs resource scopes and response types plausability
            //////////////////////////////////////////////////////////
            if (_validatedRequest.ResponseType == Constants.ResponseTypes.IdToken)
            {
                // must include identity scopes, but no resource scopes
                if (!_validatedRequest.IsOpenIdRequest || _validatedRequest.IsResourceRequest)
                {
                    _logger.Error("Requests for id_token or id_token token response types must include identity scopes, but no resource scopes");
                    return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
                }
                
            }
            else if (_validatedRequest.ResponseType == Constants.ResponseTypes.IdTokenToken)
            {
                // must include identity scopes
                if (!_validatedRequest.IsOpenIdRequest)
                {
                    _logger.Error("Requests for id_token response type must include identity scopes");
                    return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
                }
            }
            else if (_validatedRequest.ResponseType == Constants.ResponseTypes.Token)
            {
                // must include resource scopes, but no identity scopes
                if (_validatedRequest.IsOpenIdRequest || !_validatedRequest.IsResourceRequest)
                {
                    _logger.Error("Requests for token response type must include resource scopes, but no identity scopes.");
                    return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
                }
            }

            return Valid();
        }

        private ValidationResult Invalid(ErrorTypes errorType = ErrorTypes.User, string error = Constants.AuthorizeErrors.InvalidRequest)
        {
            var result = new ValidationResult();

            result.IsError = true;
            result.Error = error;
            result.ErrorType = errorType;

            return result;
        }

        private ValidationResult Valid()
        {
            var result = new ValidationResult();
            result.IsError = false;

            return result;
        }
    }
}