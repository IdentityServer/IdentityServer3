using System.Linq;

namespace IdentityServer3.Tests
{
    internal static class TestExtensions
    {
        public static string Repeat(this string value, int count)
        {
            var parts = new string[count];
            return parts.Aggregate((x, y) => (x ?? value) + value);
        }
    }
}