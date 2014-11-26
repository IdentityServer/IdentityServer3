using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class DefaultRedirectUriValidator : IRedirectUriValidator
    {
        public Task<bool> IsRedirecUriValidAsync(Uri requestedUri, Client client)
        {
            var result = client.RedirectUris.Contains(requestedUri);

            return Task.FromResult(result);
        }

        public Task<bool> IsPostLogoutRedirecUriValidAsync(Uri requestedUri, Client client)
        {
            var result = client.PostLogoutRedirectUris.Contains(requestedUri);

            return Task.FromResult(result);
        }
    }
}
