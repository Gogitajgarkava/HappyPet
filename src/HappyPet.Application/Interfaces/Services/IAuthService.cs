using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responises;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task LogoutAllAsync(int userId, CancellationToken cancellationToken = default);
    }
}
