using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Services;

namespace MembershipReboot.IdentityServer.UserService
{
    public class DisposableUserService : IUserService, IDisposable
    {
        IUserService inner;
        IDisposable cleanup;
        public DisposableUserService(IUserService inner, IDisposable cleanup)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (cleanup == null) throw new ArgumentNullException("cleanup");

            this.inner = inner;
            this.cleanup = cleanup;
        }

        public void Dispose()
        {
            if (cleanup != null)
            {
                cleanup.Dispose();
                cleanup = null;
            }
        }

        public string Authenticate(string username, string password)
        {
            return inner.Authenticate(username, password);
        }

        public IEnumerable<Claim> GetProfileData(string sub, IEnumerable<string> requestedClaimTypes = null)
        {
            return inner.GetProfileData(sub, requestedClaimTypes);
        }
    }
}
