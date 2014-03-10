using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public interface IUserManager
    {
        Task<UserManagerMetadata> GetMetadataAsync();
        Task<UserManagerResult<QueryResult>> QueryAsync(string filter, int start, int count);
        Task<UserManagerResult<UserResult>> GetUserAsync(string subject);
        Task<UserManagerResult<CreateResult>> CreateAsync(string username, string password);
        Task<UserManagerResult> SetPasswordAsync(string subject, string password);
    }
}
