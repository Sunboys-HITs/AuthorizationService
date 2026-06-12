using AuthorizationService.Application.Features.Auth;
using AuthorizationService.Application.Models;
using AuthorizationService.Application.Services;
using AuthorizationService.Db.Models;
using AuthorizationService.Db.Repositories;

namespace AuthorizationService.Tests.Features.Auth;

public sealed class RegisterCommandHandlerTests
{
    [Fact(DisplayName = "КОГДА регистрация валидна ТОГДА создается пользователь")]
    public async Task Handle_CreatesUser()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var hasher = new Pbkdf2PasswordHasher();
        var handler = new RegisterCommandHandler(repository, hasher);
        var request = new RegisterRequest
        {
            Email = " User@Example.COM ",
            Password = "secret123",
            Role = UserRole.Manager
        };

        await handler.Handle(new RegisterCommand(request), CancellationToken.None);

        var user = await repository.GetByEmailAsync("user@example.com", CancellationToken.None);
        Assert.NotNull(user);
        Assert.Equal("User@Example.COM", user.Email);
        Assert.Equal("user@example.com", user.NormalizedEmail);
        Assert.Equal(UserRole.Manager, user.Role);
        Assert.True(hasher.Verify("secret123", user.PasswordHash));
    }

    [Fact(DisplayName = "КОГДА email уже существует ТОГДА регистрация выбрасывает ArgumentException")]
    public async Task Handle_ThrowsArgumentException_WhenEmailAlreadyExists()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var hasher = new Pbkdf2PasswordHasher();
        var handler = new RegisterCommandHandler(repository, hasher);
        var firstRequest = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "secret123",
            Role = UserRole.User
        };
        var secondRequest = new RegisterRequest
        {
            Email = "USER@example.com",
            Password = "secret123",
            Role = UserRole.Admin
        };
        await handler.Handle(new RegisterCommand(firstRequest), CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new RegisterCommand(secondRequest), CancellationToken.None));
    }
}
