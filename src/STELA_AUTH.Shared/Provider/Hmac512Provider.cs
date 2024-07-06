using System.Security.Cryptography;
using System.Text;

namespace STELA_AUTH.Shared.Provider
{
    public class Hmac512Provider
    {
        private readonly string _key;

        public Hmac512Provider(string key)
        {
            _key = key;
        }

        public string Compute(string value)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_key);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(hash);
        }
    }
}