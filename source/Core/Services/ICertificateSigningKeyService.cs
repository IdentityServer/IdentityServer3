using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICertificateSigningKeyService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<X509Certificate2> GetCertificate(JsonWebKey key);
    }
}