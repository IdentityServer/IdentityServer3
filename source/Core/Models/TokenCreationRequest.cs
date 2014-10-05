using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class TokenCreationRequest
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public ClaimsPrincipal Subject { get; set; }
        public Client Client { get; set; }
        public IEnumerable<Scope> Scopes { get; set; }
        public ValidatedRequest ValidatedRequest { get; set; }

        public bool IncludeAllIdentityClaims { get; set; }
        public string AccessTokenToHash { get; set; }
        public string AuthorizationCodeToHash { get; set; }

        public void Validate()
        {
            if (Client == null) LogAndStop("client");
            if (Scopes == null) LogAndStop("scopes");
            if (ValidatedRequest == null) LogAndStop("validatedRequest");
        }

        private void LogAndStop(string name)
        {
            Logger.ErrorFormat("{0} is null", name);
            throw new ArgumentNullException(name);
        }
    }
}