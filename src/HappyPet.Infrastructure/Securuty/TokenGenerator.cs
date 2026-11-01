using HappyPet.Application.Interfaces.Security;
using HappyPet.Application.Options;
using HappyPet.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HappyPet.Infrastructure.Securuty
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly IOptions<JwtOptions> _options;

        public TokenGenerator(IOptions<JwtOptions> options)
        {
            _options = options;
        }

        public AccessTokenResult GenerateToken(User user)
        {
            var options = _options.Value;

            if (string.IsNullOrWhiteSpace(options.Secret))
            {
                throw new InvalidOperationException("Jwt:Secret კონფიგურაცია აუცილებელია.");
            }

            if (options.AccessTokenMinutes <= 0)
            {
                throw new InvalidOperationException("Jwt:AccessTokenMinutes უნდა იყოს დადებითი რიცხვი.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.UserData, user.FirstName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(options.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new AccessTokenResult(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt,
                options.AccessTokenMinutes * 60);
        }
    }
}
