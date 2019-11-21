using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Escon.SisctNET.Web.Security.Configuration
{
    public class SigningConfigurations
    {
        public SigningConfigurations()
        {
            using (var provider = new RSACryptoServiceProvider(2048))
            {
                Key = new RsaSecurityKey(provider.ExportParameters(true));
            }

            SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.RsaSha256Signature);
        }

        public SecurityKey Key { get; }

        public SigningCredentials SigningCredentials { get; }
    }
}
