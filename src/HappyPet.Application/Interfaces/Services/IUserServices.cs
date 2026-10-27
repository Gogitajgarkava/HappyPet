using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IUserServices
    {
        Task<UserResponse> RegisterAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
        Task ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default);
        Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
        Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    }
}
