using HappyPet.Application.Interfaces.Security;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace HappyPet.Infrastructure.Securuty
{
    public class OtpHasher : IOtpHasher
    {
        private readonly byte[] _key;

        public OtpHasher(IConfiguration configuration)
        {
            var hashKey = configuration["Otp:HashKey"];

            if (string.IsNullOrWhiteSpace(hashKey) || hashKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "Otp:HashKey კონფიგურაცია აუცილებელია და უნდა იყოს მინიმუმ 32 სიმბოლო.");
            }

            _key = Encoding.UTF8.GetBytes(hashKey);
        }

        public string Hash(string code)
        {
            using var hmac = new HMACSHA256(_key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToHexString(hash);
        }

        public bool Verify(string code, string hash)
        {
            try
            {
                var expected = Convert.FromHexString(hash);
                var actual = Convert.FromHexString(Hash(code));
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
