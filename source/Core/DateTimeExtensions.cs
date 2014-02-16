using System;

namespace Thinktecture.IdentityServer.Core
{
    public static class DateTimeExtensions
    {
        public static bool HasExpired(this DateTime creationTime, int seconds)
        {
            return (DateTime.UtcNow > creationTime.AddSeconds(seconds));
        }
    }
}
