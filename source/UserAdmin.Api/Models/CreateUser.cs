using System.ComponentModel.DataAnnotations;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Models
{
    public class CreateUser
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
