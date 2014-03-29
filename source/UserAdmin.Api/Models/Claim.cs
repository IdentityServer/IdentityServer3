using System.ComponentModel.DataAnnotations;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Models
{
    public class Claim
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Value { get; set; }
    }
}
