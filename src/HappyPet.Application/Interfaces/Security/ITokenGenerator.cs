using HappyPet.Domain.Entities;

namespace HappyPet.Application.Interfaces.Security
{
    public record AccessTokenResult(string Token, DateTime ExpiresAt, int ExpiresIn);

    public interface ITokenGenerator
    {
        AccessTokenResult GenerateToken(User user);
    }
}
