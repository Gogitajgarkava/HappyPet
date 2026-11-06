using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Security;
using HappyPet.Application.Options;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;

namespace HappyPet.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenGenerator> _tokenGenerator = new();
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGenerator = new();
    private readonly Mock<IRefreshTokenHasher> _refreshTokenHasher = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public AuthServiceTests()
    {
        _refreshTokenGenerator.Setup(g => g.GenerateToken()).Returns("raw-refresh-token");
        _refreshTokenHasher.Setup(h => h.Hash("raw-refresh-token")).Returns("hashed-refresh-token");
        _tokenGenerator
            .Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns(new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15), 900));
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private AuthService CreateService() => new(
        _userRepository.Object,
        _passwordHasher.Object,
        _tokenGenerator.Object,
        _refreshTokenGenerator.Object,
        _refreshTokenHasher.Object,
        _refreshTokenRepository.Object,
        _unitOfWork.Object,
        Options.Create(new JwtOptions
        {
            Secret = new string('s', 32),
            Issuer = "HappyPetApi",
            Audience = "HappyPetClient",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 30,
            RefreshTokenHashKey = new string('h', 32)
        }));

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsUnauthorized()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync("missing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().LoginAsync(new LoginRequest
            {
                Email = "missing@example.com",
                Password = "secret123"
            }));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ThrowsUnauthorized()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser());
        _passwordHasher
            .Setup(h => h.VerifyPassword("wrong", It.IsAny<string>()))
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().LoginAsync(new LoginRequest
            {
                Email = "owner@example.com",
                Password = "wrong"
            }));
    }

    [Fact]
    public async Task LoginAsync_WhenEmailIsNotVerified_ThrowsEmailNotVerified()
    {
        var user = BuildUser(isEmailVerified: false);
        _userRepository
            .Setup(r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(h => h.VerifyPassword("secret123", user.Password))
            .Returns(true);

        await Assert.ThrowsAsync<EmailNotVerifiedException>(
            () => CreateService().LoginAsync(new LoginRequest
            {
                Email = "owner@example.com",
                Password = "secret123"
            }));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokensAndStoresHashedRefreshToken()
    {
        var user = BuildUser();
        _userRepository
            .Setup(r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(h => h.VerifyPassword("secret123", user.Password))
            .Returns(true);

        var result = await CreateService().LoginAsync(new LoginRequest
        {
            Email = "owner@example.com",
            Password = "secret123"
        });

        Assert.Equal("access-token", result.Response.AccessToken);
        Assert.Equal(900, result.Response.ExpiresIn);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
        Assert.True(result.RefreshTokenExpiresAt > DateTime.UtcNow);

        _refreshTokenRepository.Verify(
            r => r.AddAsync(
                It.Is<RefreshToken>(t =>
                    t.UserId == user.Id &&
                    t.TokenHash == "hashed-refresh-token" &&
                    t.RevokedAt == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_NormalizesEmailBeforeLookup()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().LoginAsync(new LoginRequest
            {
                Email = "  OWNER@Example.COM  ",
                Password = "secret123"
            }));

        _userRepository.Verify(
            r => r.GetByEmailAsync("owner@example.com", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsBlank_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().RefreshAsync("  "));
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsUnknown_ThrowsUnauthorized()
    {
        _refreshTokenHasher.Setup(h => h.Hash("unknown")).Returns("unknown-hash");
        _refreshTokenRepository
            .Setup(r => r.GetByHashWithUserAsync("unknown-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().RefreshAsync("unknown"));
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsExpired_RevokesItAndThrows()
    {
        var token = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            TokenHash = "expired-hash",
            FamilyId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };
        _refreshTokenHasher.Setup(h => h.Hash("expired")).Returns("expired-hash");
        _refreshTokenRepository
            .Setup(r => r.GetByHashWithUserAsync("expired-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().RefreshAsync("expired"));

        Assert.Equal("expired", token.RevocationReason);
        Assert.NotNull(token.RevokedAt);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
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
}
