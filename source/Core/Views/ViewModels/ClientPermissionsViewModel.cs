using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Views
{
    public class ClientPermissionsViewModel : CommonViewModel
    {
        public string LogoutUrl { get; set; }
        public IEnumerable<ClientPermission> Clients { get; set; }
        public AntiForgeryHiddenInputViewModel AntiForgery { get; set; }
    }

    public class ClientPermission
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientLogoUrl { get; set; }
        public string RevokePermissionUrl { get; set; }
        public IEnumerable<PermissionDescription> IdentityPermissions { get; set; }
        public IEnumerable<PermissionDescription> ResourcePermissions { get; set; }
    }

    public class PermissionDescription
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}
