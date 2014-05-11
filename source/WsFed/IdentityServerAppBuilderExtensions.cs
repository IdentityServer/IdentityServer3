/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
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