using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IRedirectUriValidator
    {
        Task<bool> IsRedirecUriValidAsync(Uri requestedUri, Client client);
        Task<bool> IsPostLogoutRedirecUriValidAsync(Uri requestedUri, Client client);
    }
}