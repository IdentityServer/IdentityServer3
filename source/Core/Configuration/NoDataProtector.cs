namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class NoDataProtector : IDataProtector
    {
        public byte[] Protect(byte[] data, string entropy = null)
        {
            return data;
        }

        public byte[] Unprotect(byte[] data, string entropy = null)
        {
            return data;
        }
    }
}