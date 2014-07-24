/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class CorsPolicy
    {
        public CorsPolicy()
        {
            AllowedOrigins = new List<string>();
        }
        
        public ICollection<string> AllowedOrigins { get; private set; }
        public Func<string, Task<bool>> PolicyCallback { get; set; }

        public static readonly CorsPolicy AllowAll = new CorsPolicy()
        {
            PolicyCallback = origin => Task.FromResult(true)
        };
    }
}
