using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Code;
using HappyPet.Application.Interfaces.Mail;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Security;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Moq;

namespace HappyPet.Tests.Services;

public class UserServicesTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<ICodeGenerator> _codeGenerator = new();
    private readonly Mock<IOtpHasher> _otpHasher = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();

    public UserServicesTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _codeGenerator.Setup(g => g.GenerateCode(6)).Returns("123456");
        _otpHasher.Setup(h => h.Hash("123456")).Returns("otp-hash");
    }

    private UserServices CreateService() => new(
        _userRepository.Object,
        _passwordHasher.Object,
        _unitOfWork.Object,
        _emailSender.Object,
        _codeGenerator.Object,
        _otpHasher.Object,
        _refreshTokenRepository.Object);

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsInvalidOperation()
    {
        _userRepository
            .Setup(r => r.ExistsByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().RegisterAsync(BuildCreateUserRequest()));

        _emailSender.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUnverifiedUserAndSendsOtp()
    {
        _userRepository
            .Setup(r => r.ExistsByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.ExistsByPersonalIdAsync("01000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => user.Id = 1)
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(r => r.GetLatestOtpCodeAsync(1, OtpPurpose.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOtpCode?)null);
        _userRepository
            .Setup(r => r.CountOtpCodesSinceAsync(1, OtpPurpose.EmailVerification, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _userRepository
            .Setup(r => r.GetActiveOtpCodesAsync(1, OtpPurpose.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserOtpCode>());
        _passwordHasher
            .Setup(h => h.HashPassword("secret123"))
            .Returns("hashed-password");

        var result = await CreateService().RegisterAsync(BuildCreateUserRequest());

        Assert.False(result.IsEmailVerified);
        Assert.Equal("owner@example.com", result.Email);

        _userRepository.Verify(
            r => r.AddOtpCodeAsync(
                It.Is<UserOtpCode>(c =>
                    c.UserId == 1 &&
                    c.Purpose == OtpPurpose.EmailVerification &&
                    c.CodeHash == "otp-hash"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _emailSender.Verify(
            e => e.SendEmailAsync(
                "owner@example.com",
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains("123456")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_WithValidOtp_VerifiesUserAndConsumesCode()
    {
        var user = BuildUser(isEmailVerified: false);
        var code = BuildOtpCode(user.Id, OtpPurpose.EmailVerification);

        _userRepository
            .Setup(r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository
            .Setup(r => r.GetLatestOtpCodeAsync(user.Id, OtpPurpose.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);
        _userRepository
            .Setup(r => r.GetActiveOtpCodesAsync(user.Id, OtpPurpose.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserOtpCode> { code });
        _otpHasher.Setup(h => h.Verify("123456", "otp-hash")).Returns(true);

        await CreateService().VerifyEmailAsync(new VerifyEmailRequest
        {
            Email = "owner@example.com",
            Code = "123456"
        });

        Assert.True(user.IsEmailVerified);
        Assert.NotNull(user.EmailVerifiedAt);
        Assert.NotNull(code.ConsumedAt);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_WithWrongOtp_IncrementsAttemptsAndThrows()
    {
        var user = BuildUser(isEmailVerified: false);
        var code = BuildOtpCode(user.Id, OtpPurpose.EmailVerification);

        _userRepository
            .Setup(r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository
            .Setup(r => r.GetLatestOtpCodeAsync(user.Id, OtpPurpose.EmailVerification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);
        _otpHasher.Setup(h => h.Verify("000000", "otp-hash")).Returns(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().VerifyEmailAsync(new VerifyEmailRequest
            {
                Email = "owner@example.com",
                Code = "000000"
            }));

        Assert.Equal(1, code.AttemptCount);
        Assert.False(user.IsEmailVerified);
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenAlreadyVerified_Throws()
    {
        var user = BuildUser(isEmailVerified: true);
        _userRepository
            .Setup(r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().VerifyEmailAsync(new VerifyEmailRequest
            {
                Email = "owner@example.com",
                Code = "123456"
            }));

        _userRepository.Verify(
            r => r.GetLatestOtpCodeAsync(
                It.IsAny<int>(),
                It.IsAny<OtpPurpose>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidOtp_ChangesPasswordAndRevokesSessions()
    {
        var user = BuildUser();
        var code = BuildOtpCode(user.Id, OtpPurpose.PasswordReset);
        var refreshToken = new RefreshToken
        {
            Id = 5,
            UserId = user.Id,
            TokenHash = "refresh-hash",
            FamilyId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(20)
        };

        _userRepository
            .Setup(r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository
            .Setup(r => r.GetLatestOtpCodeAsync(user.Id, OtpPurpose.PasswordReset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);
        _userRepository
            .Setup(r => r.GetActiveOtpCodesAsync(user.Id, OtpPurpose.PasswordReset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserOtpCode> { code });
        _otpHasher.Setup(h => h.Verify("123456", "otp-hash")).Returns(true);
        _passwordHasher.Setup(h => h.HashPassword("new-secret")).Returns("new-hash");
        _refreshTokenRepository
            .Setup(r => r.GetActiveByUserAsync(user.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken });

        await CreateService().ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = "owner@example.com",
            Code = "123456",
            NewPassword = "new-secret"
        });

        Assert.Equal("new-hash", user.Password);
        Assert.NotNull(refreshToken.RevokedAt);
        Assert.Equal("password-reset", refreshToken.RevocationReason);
        Assert.NotNull(code.ConsumedAt);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenUserDoesNotExist_DoesNotSendEmail()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync("missing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await CreateService().ForgotPasswordAsync(new ForgotPasswordRequest
        {
            Email = "missing@example.com"
        });

        _emailSender.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CreateUserRequest BuildCreateUserRequest()
    {
        return new CreateUserRequest
        {
            PersonalId = "01000000001",
            FirstName = "გიორგი",
            LastName = "ჯღარკავა",
            BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = "owner@example.com",
            Password = "secret123",
            PhoneNumber = "+995510000000"
        };
    }

    private static User BuildUser(bool isEmailVerified = true)
    {
        return new User
        {
            Id = 1,
            PersonalId = "01000000001",
            FirstName = "გიორგი",
            LastName = "ჯღარკავა",
            Email = "owner@example.com",
            Password = "stored-hash",
            PhoneNumber = "+995510000000",
            IsEmailVerified = isEmailVerified,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static UserOtpCode BuildOtpCode(int userId, OtpPurpose purpose)
    {
        return new UserOtpCode
        {
            Id = 10,
            UserId = userId,
            Purpose = purpose,
            CodeHash = "otp-hash",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
    }
}
