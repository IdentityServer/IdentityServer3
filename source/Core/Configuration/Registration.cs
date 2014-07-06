using System;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class Registration
    {
        public Type Type { get; set; }
        public Func<object> TypeFactory { get; set; }

        public static Registration RegisterType(Type type)
        {
            return new Registration
            {
                Type = type
            };
        }

        public static Registration RegisterFactory(Func<object> typeFunc)
        {
            return new Registration
            {
                TypeFactory = typeFunc
            };
        }
    }
}