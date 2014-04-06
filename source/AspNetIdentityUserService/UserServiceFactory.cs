/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.AspNetIdentity
{
    public class UserServiceFactory
    {
        public static IUserService Factory()
        {
            var db = new IdentityDbContext<IdentityUser>("DefaultConnection");
            var store = new UserStore<IdentityUser>(db);
            var mgr = new UserManager<IdentityUser>(store);
            var userSvc = new UserService<IdentityUser>(mgr, db);
            return userSvc;
        }

        static UserServiceFactory()
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<IdentityDbContext>());
        }
    }
}
