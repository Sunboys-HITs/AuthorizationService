using System.Security.Claims;
using AuthorizationService.Application.Features.Auth;
using AuthorizationService.Db.Models;
using AuthorizationService.Db.Repositories;

namespace AuthorizationService.Tests.Features.Auth;

public sealed class ValidateTokenQueryHandlerTests
{
    [Fact(DisplayName = "КОГДА claim и пользователь существуют ТОГДА validate возвращает userId")]
    public async Task Handle_ReturnsUserId_WhenClaimAndUserExist()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateUser(UserRole.User);
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        var handler = new ValidateTokenQueryHandler(repository);

        var result = await handler.Handle(
            new ValidateTokenQuery(CreatePrincipal(user.Id)),
            CancellationToken.None);

        Assert.Equal(user.Id, result);
    }

    [Fact(DisplayName = "КОГДА claim отсутствует ТОГДА validate возвращает null")]
    public async Task Handle_ReturnsNull_WhenClaimIsMissing()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var handler = new ValidateTokenQueryHandler(repository);

        var result = await handler.Handle(
            new ValidateTokenQuery(new ClaimsPrincipal(new ClaimsIdentity())),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact(DisplayName = "КОГДА пользователь из claim не найден ТОГДА validate возвращает null")]
    public async Task Handle_ReturnsNull_WhenUserDoesNotExist()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var handler = new ValidateTokenQueryHandler(repository);

        var result = await handler.Handle(
            new ValidateTokenQuery(CreatePrincipal(Guid.NewGuid())),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact(DisplayName = "КОГДА role claim отсутствует ТОГДА validate возвращает null")]
    public async Task Handle_ReturnsNull_WhenRoleClaimIsMissing()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateUser(UserRole.User);
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        var handler = new ValidateTokenQueryHandler(repository);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) },
            authenticationType: "Test"));
        var result = await handler.Handle(new ValidateTokenQuery(principal), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact(DisplayName = "КОГДА role claim не совпадает с ролью пользователя ТОГДА validate возвращает null")]
    public async Task Handle_ReturnsNull_WhenRoleClaimDoesNotMatchUserRole()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateUser(UserRole.Admin);
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        var handler = new ValidateTokenQueryHandler(repository);

        var result = await handler.Handle(
            new ValidateTokenQuery(CreatePrincipal(user.Id)),
            CancellationToken.None);

        Assert.Null(result);
    }

    private static ClaimsPrincipal CreatePrincipal(Guid userId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, UserRole.User.ToString())
            },
            authenticationType: "Test"));
    }

    private static User CreateUser(UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            NormalizedEmail = "user@example.com",
            PasswordHash = "hash",
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
