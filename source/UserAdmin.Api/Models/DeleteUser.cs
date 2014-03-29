using System.ComponentModel.DataAnnotations;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Models
{
    public class DeleteUser
    {
        [Required]
        public string Subject { get; set; }
    }
}
