using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Events
{
    public static class EventIds
    {
        internal const int AuthenticationEventsStart = 1000;

        public const int SuccessfulLocalLogin = AuthenticationEventsStart + 0;
        public const int FailedLocalLogin = AuthenticationEventsStart + 1;
        public const int SuccessfulExternalLogin = AuthenticationEventsStart + 2;
        public const int FailedExternalLogin = AuthenticationEventsStart + 3;
    }
}
