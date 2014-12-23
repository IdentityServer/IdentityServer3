using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Validation.Logging
{
    class TokenValidationLog
    {
        public Dictionary<string, string> Raw { get; set; }

        public string ClientId { get; set; }
        public string ClientName { get; set; }

        public string GrantType { get; set; }
        public string AuthorizationCode { get; set; }
        public string RefreshToken { get; set; }
        
        public string Scopes { get; set; }
        public string UserName { get; set; }

        public TokenValidationLog(ValidatedTokenRequest request)
        {
            if (request.Raw != null)
            {
                Raw = request.Raw.ToDictionary();
            }

            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;
            }

            GrantType = request.GrantType;
            
            //AuthorizationCode = request.AuthorizationCode.

            RefreshToken = request.RefreshTokenHandle;

            UserName = request.UserName;

            if (request.Scopes != null)
            {
                Scopes = request.Scopes.ToSpaceSeparatedString();
            }
        }
    }
}