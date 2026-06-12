using AuthorizationService.Application.Features.Auth;
using AuthorizationService.Application.Models;
using AuthorizationService.Application.Services;
using AuthorizationService.Db.Models;
using AuthorizationService.Db.Repositories;

namespace AuthorizationService.Tests.Features.Auth;

public sealed class LoginCommandHandlerTests
{
    [Fact(DisplayName = "КОГДА логин с валидными данными ТОГДА возвращается AuthResponse")]
    public async Task Handle_ReturnsAuthResponse_WhenCredentialsAreValid()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var hasher = new Pbkdf2PasswordHasher();
        var user = CreateUser("user@example.com", hasher.Hash("secret123"));
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        var handler = new LoginCommandHandler(repository, hasher, new FakeJwtTokenGenerator("test-token"));
        var request = new LoginRequest
        {
            Email = "USER@example.com",
            Password = "secret123"
        };

        var response = await handler.Handle(new LoginCommand(request), CancellationToken.None);

        Assert.Equal(user.Id, response.UserId);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(user.Role, response.Role);
        Assert.Equal("test-token", response.Token);
    }

    [Fact(DisplayName = "КОГДА пользователь не найден ТОГДА логин выбрасывает KeyNotFoundException")]
    public async Task Handle_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var handler = new LoginCommandHandler(
            repository,
            new Pbkdf2PasswordHasher(),
            new FakeJwtTokenGenerator("test-token"));
        var request = new LoginRequest
        {
            Email = "missing@example.com",
            Password = "secret123"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new LoginCommand(request), CancellationToken.None));
    }

    [Fact(DisplayName = "КОГДА пароль неверный ТОГДА логин выбрасывает UnauthorizedAccessException")]
    public async Task Handle_ThrowsUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var hasher = new Pbkdf2PasswordHasher();
        await repository.AddAsync(CreateUser("user@example.com", hasher.Hash("secret123")), CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        var handler = new LoginCommandHandler(repository, hasher, new FakeJwtTokenGenerator("test-token"));
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "wrong-password"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginCommand(request), CancellationToken.None));
    }

    private static User CreateUser(string email, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = UserRole.User,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly string _token;

        public FakeJwtTokenGenerator(string token)
        {
            _token = token;
        }

        public string Generate(User user)
        {
            return _token;
        }
    }
}
