using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public static class IdentityServerAppBuilderExtensions
    {
        public static IdentityServerAppBuilder WithWsFed(this IdentityServerAppBuilder app)
        {
            return app;
        }
    }
}
