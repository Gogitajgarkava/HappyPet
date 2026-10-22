using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IUserServices
    {
        Task<UserResponse> RegisterAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
