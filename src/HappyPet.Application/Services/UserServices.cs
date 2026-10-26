using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HappyPet.Application.Interfaces.Security;


namespace HappyPet.Application.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public UserServices(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserResponse> RegisterAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var personalId = request.PersonalId.Trim();

            if (request.BirthDate == default)
            {
                throw new InvalidOperationException("დაბადების თარიღი სავალდებულოა.");
            }

            if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            {
                throw new InvalidOperationException("მითითებული ელ-ფოსტით მომხმარებელი უკვე რეგისტრირებულია.");
            }
            if (await _userRepository.ExistsByPersonalIdAsync(personalId, cancellationToken))
            {
                throw new InvalidOperationException("მითითებული პირადი ნომრით მომხმარებელი უკვე რეგისტრირებულია.");
            }

            var newUser = new User
            {
                PersonalId = personalId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Password = _passwordHasher.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber.Trim(),
                Address = request.Address?.Trim(),
                Role = UserRole.User,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                BirthDate = ToUtc(request.BirthDate)
            };

            await _userRepository.AddAsync(newUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(newUser);
        }

        public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("მომხმარებელი არ მოიძებნა.");
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = request.PhoneNumber.Trim();
            user.Address = request.Address?.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(user);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("მომხმარებელი არ მოიძებნა.");
            }

            _userRepository.Delete(user);
            await _unitOfWork.SaveChangesAsync();
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                PersonalId = user.PersonalId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDate = user.BirthDate,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsVerified = user.IsVerified,
                Address = user.Address,
                ProfileImageUrl = null,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
