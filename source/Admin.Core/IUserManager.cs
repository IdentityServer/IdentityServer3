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
        Task<UserManagerResult> CreateAsync(string username, string password);
        Task<UserManagerResult<QueryResult>> QueryAsync(string filter, int start, int count);
    }
}
