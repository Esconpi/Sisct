using System.Security.Cryptography;
using System.Text;

namespace Escon.SisctNET.Web.Crypto
{
    public class HashManager
    {
        private readonly HashAlgorithm _algorithm;

        public HashManager()
        {
            _algorithm = SHA512.Create();
        }

        public HashManager(HashAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        public string GenerateHash(string value)
        {
            var encodedValue = Encoding.UTF8.GetBytes(value);
            var valueHashed = _algorithm.ComputeHash(encodedValue);
            var sb = new StringBuilder();

            foreach (var ch in valueHashed)
                sb.Append(ch.ToString("X2"));

            return sb.ToString();
        }
    }
}
