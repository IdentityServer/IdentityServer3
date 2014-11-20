using System;
using System.Threading.Tasks;
using Xunit;

namespace Thinktecture.IdentityServer.Tests
{
    public class XunitExtensions
    {
        public async static Task<T> ThrowsAsync<T>(Func<Task> testCode) where T : Exception
        {
            try
            {
                await testCode();
                Assert.Throws<T>(() => { }); // Use xUnit's default behavior.
            }
            catch (T exception)
            {
                return exception;
            }
            return null;
        }
    }
}
