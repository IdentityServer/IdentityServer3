using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services
{
    public interface IPermissionsStore
    {
        Task<IEnumerable<Consent>> LoadAllAsync(string subject);
        Task RevokeAsync(string subject, string client);
    }
}