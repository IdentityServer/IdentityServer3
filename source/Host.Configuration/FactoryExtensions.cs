using IdentityServer3.Core.Services;
using IdentityServer3.Host.Config;

namespace IdentityServer3.Core.Configuration
{
    static class FactoryExtensions
    {
        public static IdentityServerServiceFactory AddCustomGrantValidators(this IdentityServerServiceFactory factory)
        {
            factory.CustomGrantValidators.Add(
                    new Registration<ICustomGrantValidator>(typeof(CustomGrantValidator)));
            factory.CustomGrantValidators.Add(
                new Registration<ICustomGrantValidator>(typeof(AnotherCustomGrantValidator)));

            return factory;
        }
    }
}