using HappyPet.Application.Interfaces.Security;
using HappyPet.Application.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace HappyPet.Infrastructure.Securuty
{
    public class RefreshTokenHasher : IRefreshTokenHasher
    {
        private readonly byte[] _key;

        public RefreshTokenHasher(IOptions<JwtOptions> options)
        {
            var hashKey = options.Value.RefreshTokenHashKey;

            if (string.IsNullOrWhiteSpace(hashKey) || hashKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:RefreshTokenHashKey კონფიგურაცია აუცილებელია და უნდა იყოს მინიმუმ 32 სიმბოლო.");
            }

            _key = Encoding.UTF8.GetBytes(hashKey);
        }

        public string Hash(string token)
        {
            using var hmac = new HMACSHA256(_key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hash);
        }

        public bool Verify(string token, string hash)
        {
            try
            {
                var expected = Convert.FromHexString(hash);
                var actual = Convert.FromHexString(Hash(token));
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
