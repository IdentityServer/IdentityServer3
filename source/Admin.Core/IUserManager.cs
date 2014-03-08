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
        Task<Result> CreateAsync(string username, IEnumerable<Claim> claims);
    }
}
