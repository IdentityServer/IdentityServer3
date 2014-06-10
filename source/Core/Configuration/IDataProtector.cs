namespace Thinktecture.IdentityServer.Core.Configuration
{
    public interface IDataProtector
    {
        byte[] Protect(byte[] data, string entropy = "");
        byte[] Unprotect(byte[] data, string entropy = "");
    }
}