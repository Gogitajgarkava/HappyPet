using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Code;
using HappyPet.Application.Interfaces.Mail;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Security;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Services
{
    public class UserServices : IUserServices
    {
        private const int OtpLength = 6;
        private const int OtpMaxAttempts = 5;
        private const int OtpMaxSendsPerWindow = 3;
        private const string InvalidOtpMessage = "კოდი არასწორია ან ვადა გაუვიდა.";

        private static readonly TimeSpan EmailVerificationLifetime = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan OtpRateLimitWindow = TimeSpan.FromMinutes(15);

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailService;
        private readonly ICodeGenerator _codeGenerator;
        private readonly IOtpHasher _otpHasher;

        public UserServices(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            IEmailSender emailService,
            ICodeGenerator codeGenerator,
            IOtpHasher otpHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _codeGenerator = codeGenerator;
            _otpHasher = otpHasher;
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
                IsEmailVerified = false,
                EmailVerifiedAt = null,
                CreatedAt = DateTime.UtcNow,
                BirthDate = ToUtc(request.BirthDate)
            };

            await _userRepository.AddAsync(newUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await IssueOtpAsync(
                    newUser,
                    OtpPurpose.EmailVerification,
                    EmailVerificationLifetime,
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Do not leave an account behind if the verification email could not be sent.
                _userRepository.Delete(newUser);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
                throw;
            }

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

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(
                request.Email.Trim().ToLowerInvariant(),
                cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException(InvalidOtpMessage);
            }

            if (user.IsEmailVerified)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var code = await _userRepository.GetLatestOtpCodeAsync(
                user.Id,
                OtpPurpose.EmailVerification,
                cancellationToken);

            if (!IsOtpUsable(code, now))
            {
                throw new InvalidOperationException(InvalidOtpMessage);
            }

            if (!_otpHasher.Verify(request.Code.Trim(), code!.CodeHash))
            {
                await RegisterFailedAttemptAsync(code, now, cancellationToken);
                throw new InvalidOperationException(InvalidOtpMessage);
            }

            user.IsEmailVerified = true;
            user.EmailVerifiedAt = now;
            user.UpdatedAt = now;

            await ConsumeActiveOtpCodesAsync(
                user.Id,
                OtpPurpose.EmailVerification,
                now,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(
                request.Email.Trim().ToLowerInvariant(),
                cancellationToken);

            if (user == null || user.IsEmailVerified)
            {
                return;
            }

            try
            {
                await IssueOtpAsync(
                    user,
                    OtpPurpose.EmailVerification,
                    EmailVerificationLifetime,
                    cancellationToken);
            }
            catch (OtpRateLimitException)
            {
                // Generic response: do not reveal rate limits or account existence.
            }
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(
                request.Email.Trim().ToLowerInvariant(),
                cancellationToken);

            if (user == null)
            {
                return;
            }

            try
            {
                await IssueOtpAsync(
                    user,
                    OtpPurpose.PasswordReset,
                    PasswordResetLifetime,
                    cancellationToken);
            }
            catch (OtpRateLimitException)
            {
                // Generic response: do not reveal rate limits or account existence.
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(
                request.Email.Trim().ToLowerInvariant(),
                cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException(InvalidOtpMessage);
            }

            var now = DateTime.UtcNow;
            var code = await _userRepository.GetLatestOtpCodeAsync(
                user.Id,
                OtpPurpose.PasswordReset,
                cancellationToken);

            if (!IsOtpUsable(code, now))
            {
                throw new InvalidOperationException(InvalidOtpMessage);
            }

            if (!_otpHasher.Verify(request.Code.Trim(), code!.CodeHash))
            {
                await RegisterFailedAttemptAsync(code, now, cancellationToken);
                throw new InvalidOperationException(InvalidOtpMessage);
            }

            user.Password = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = now;

            await ConsumeActiveOtpCodesAsync(
                user.Id,
                OtpPurpose.PasswordReset,
                now,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task IssueOtpAsync(
            User user,
            OtpPurpose purpose,
            TimeSpan lifetime,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var latestCode = await _userRepository.GetLatestOtpCodeAsync(
                user.Id,
                purpose,
                cancellationToken);

            if (latestCode != null && now - latestCode.CreatedAt < OtpResendCooldown)
            {
                throw new OtpRateLimitException("კოდის ხელახლა გაგზავნა შესაძლებელია მხოლოდ 60 წამის შემდეგ.");
            }

            var sentCount = await _userRepository.CountOtpCodesSinceAsync(
                user.Id,
                purpose,
                now - OtpRateLimitWindow,
                cancellationToken);

            if (sentCount >= OtpMaxSendsPerWindow)
            {
                throw new OtpRateLimitException("ძალიან ბევრი მოთხოვნა. სცადეთ მოგვიანებით.");
            }

            await ConsumeActiveOtpCodesAsync(user.Id, purpose, now, cancellationToken);

            var code = _codeGenerator.GenerateCode(OtpLength);
            var otpCode = new UserOtpCode
            {
                UserId = user.Id,
                Purpose = purpose,
                CodeHash = _otpHasher.Hash(code),
                ExpiresAt = now.Add(lifetime),
                CreatedAt = now
            };

            await _userRepository.AddOtpCodeAsync(otpCode, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var subject = purpose == OtpPurpose.EmailVerification
                ? "HappyPet — ელ-ფოსტის დადასტურება"
                : "HappyPet — პაროლის აღდგენა";

            var body = purpose == OtpPurpose.EmailVerification
                ? $"HappyPet-ის ელ-ფოსტის დასადასტურებლად შეიყვანეთ კოდი: {code}\n\nკოდი მოქმედია 15 წუთის განმავლობაში."
                : $"პაროლის აღსადგენად შეიყვანეთ კოდი: {code}\n\nკოდი მოქმედია 10 წუთის განმავლობაში. თუ ეს მოთხოვნა თქვენ არ გამოგზავნიათ, უგულებელყავით წერილი.";

            await _emailService.SendEmailAsync(user.Email, subject, body, cancellationToken);
        }

        private async Task ConsumeActiveOtpCodesAsync(
            int userId,
            OtpPurpose purpose,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var activeCodes = await _userRepository.GetActiveOtpCodesAsync(userId, purpose, cancellationToken);

            foreach (var activeCode in activeCodes)
            {
                activeCode.ConsumedAt = now;
                activeCode.UpdatedAt = now;
                _userRepository.UpdateOtpCode(activeCode);
            }
        }

        private async Task RegisterFailedAttemptAsync(
            UserOtpCode code,
            DateTime now,
            CancellationToken cancellationToken)
        {
            code.AttemptCount++;
            code.UpdatedAt = now;
            _userRepository.UpdateOtpCode(code);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static bool IsOtpUsable(UserOtpCode? code, DateTime now)
        {
            return code != null
                && code.ConsumedAt == null
                && code.ExpiresAt > now
                && code.AttemptCount < OtpMaxAttempts;
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
                IsEmailVerified = user.IsEmailVerified,
                EmailVerifiedAt = user.EmailVerifiedAt,
                Address = user.Address,
                ProfileImageUrl = null,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
