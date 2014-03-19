using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
