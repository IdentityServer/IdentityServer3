using System.ComponentModel.DataAnnotations;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class LoginCredentials
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
