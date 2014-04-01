/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.MembershipReboot
{
    public class UserServiceFactory
    {
        public static IUserService Factory()
        {
            var repo = new DefaultUserAccountRepository();
            var userAccountService = new UserAccountService(config, repo);
            var userSvc = new UserService<UserAccount>(userAccountService, repo);
            return userSvc;
        }

        static MembershipRebootConfiguration config;
        static UserServiceFactory()
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());

            config = new MembershipRebootConfiguration();
            config.PasswordHashingIterationCount = 50000;
            config.AllowLoginAfterAccountCreation = true;
            config.RequireAccountVerification = false;
        }
    }
}
