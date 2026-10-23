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
            if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken)) 
            {
                throw new InvalidOperationException("მითითებული ელ-ფოსტით მომხმარებელი უკვე რეგისტრირებულია.");
            }
            if (await _userRepository.ExistsByPersonalIdAsync(request.PersonalId, cancellationToken)) 
            {
                throw new InvalidOperationException("მითითებული პირადი ნომრით მომხმარებელი უკვე რეგისტრირებულია.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                PersonalId = request.PersonalId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = _passwordHasher.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Role = UserRole.User,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                BirthDate = request.BirthDate
               
            };
            await _userRepository.AddAsync(newUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(newUser);
        }

        

        public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            return user ==null ? null : MapToResponse(user);
        }

        public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("მომხმარებელი არ მოიძებნა.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(user);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
           var user = _userRepository.GetByIdAsync(id, cancellationToken).Result;
            if (user == null)
            {
                throw new InvalidOperationException("მომხმარებელი არ მოიძებნა.");
            }
            _userRepository.Delete(user);
            return _unitOfWork.SaveChangesAsync();
        }
        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                PersonalId = user.PersonalId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsVerified = user.IsVerified,
                Address = user.Address,
                CreatedAt = DateTime.Now
            };
        }



    }
}
