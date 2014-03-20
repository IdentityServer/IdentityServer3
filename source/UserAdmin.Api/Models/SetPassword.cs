using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
