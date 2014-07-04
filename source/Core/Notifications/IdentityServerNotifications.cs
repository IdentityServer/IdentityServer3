using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Notifications
{
    public class IdentityServerNotifications
    {
        public IdentityServerNotifications()
        {
            ExternalUserCreated = notification => Task.FromResult(0);
        }

        /// <summary>
        /// Invoked when a external user is created
        /// </summary>
        public Func<ExternalUserCreatedNotification, Task> ExternalUserCreated { get; set; }

    }
}
