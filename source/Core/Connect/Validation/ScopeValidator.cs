using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ScopeValidator
    {
        private ILogger _logger;

        public bool ContainsOpenIdScopes { get; private set; }
        public bool ContainsResourceScopes { get; private set; }
        public List<string> Audiences { get; private set; }
        public List<Scope> Scopes { get; private set; }

        public ScopeValidator(ILogger logger)
        {
            _logger = logger;

            Scopes = new List<Scope>();
            Audiences = new List<string>();
        }

        public void SetConsentedScopes(IEnumerable<string> consentedScopes)
        {
            consentedScopes = consentedScopes ?? Enumerable.Empty<string>();
            
            Scopes.RemoveAll(scope => !scope.Required && !consentedScopes.Contains(scope.Name));
        }

        public bool AreScopesValid(IEnumerable<string> requestedScopes, IEnumerable<Scope> availableScopes)
        {
            foreach (var requestedScope in requestedScopes)
            {
                var scopeDetail = availableScopes.FirstOrDefault(s => s.Name == requestedScope);

                if (scopeDetail == null)
                {
                    _logger.ErrorFormat("Invalid scope: {0}", requestedScope);
                    return false;
                }

                if (scopeDetail.IsOpenIdScope)
                {
                    ContainsOpenIdScopes = true;
                }
                else
                {
                    ContainsResourceScopes = true;
                }

                Scopes.Add(scopeDetail);
            }

            return true;
        }

        public List<string> ParseScopes(string scopes)
        {
            if (scopes.IsMissing())
            {
                return null;
            }

            _logger.InformationFormat("scopes: {0}", scopes);

            scopes = scopes.Trim();
            var parsedScopes = scopes.Split(' ').Distinct().ToList();
            
            if (parsedScopes.Count > 0)
            {
                return parsedScopes;
            }

            return null;
        }

        public bool AreScopesAllowed(Client client, IEnumerable<string> requestedScopes)
        {
            if (client.ScopeRestrictions == null || client.ScopeRestrictions.Count == 0)
            {
                _logger.Information("All scopes allowed for client");
                return true;
            }
            else
            {
                _logger.Information("Allowed scopes for client client: " + client.ScopeRestrictions.ToSpaceSeparatedString());

                foreach (var scope in requestedScopes)
                {
                    if (!client.ScopeRestrictions.Contains(scope))
                    {
                        _logger.ErrorFormat("Requested scope not allowed: {0}", scope);
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsResponseTypeValid(string responseType)
        {
            if (responseType == Constants.ResponseTypes.IdToken)
            {
                // must include identity scopes, but no resource scopes
                if (!ContainsOpenIdScopes || ContainsResourceScopes)
                {
                    _logger.Error("Requests for id_token or id_token token response types must include identity scopes, but no resource scopes");

                    return false;
                    //return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
                }

            }
            else if (responseType == Constants.ResponseTypes.IdTokenToken)
            {
                // must include identity scopes
                if (!ContainsOpenIdScopes)
                {
                    _logger.Error("Requests for id_token response type must include identity scopes");

                    return false;
                    //return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
                }
            }
            else if (responseType == Constants.ResponseTypes.Token)
            {
                // must include resource scopes, but no identity scopes
                if (ContainsOpenIdScopes || !ContainsResourceScopes)
                {
                    _logger.Error("Requests for token response type must include resource scopes, but no identity scopes.");

                    return false;
                    //return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
                }
            }

            return true;
        }
    }
}