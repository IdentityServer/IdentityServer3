using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public interface IUserManager
    {
        Task<UserManagerMetadata> GetMetadataAsync();
        
        Task<UserManagerResult<CreateResult>> CreateUserAsync(string username, string password);
        Task<UserManagerResult> DeleteUserAsync(string subject);
        
        Task<UserManagerResult<QueryResult>> QueryUsersAsync(string filter, int start, int count);
        Task<UserManagerResult<UserResult>> GetUserAsync(string subject);

        Task<UserManagerResult> SetPasswordAsync(string subject, string password);
        Task<UserManagerResult> AddClaimAsync(string subject, string type, string value);
        Task<UserManagerResult> DeleteClaimAsync(string subject, string type, string value);
    }
}
