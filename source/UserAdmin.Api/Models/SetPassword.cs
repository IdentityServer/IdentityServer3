using System.ComponentModel.DataAnnotations;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Models
{
    public class SetPassword
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
