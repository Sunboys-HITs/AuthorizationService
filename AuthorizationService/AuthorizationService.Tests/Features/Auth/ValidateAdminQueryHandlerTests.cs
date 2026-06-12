using System.Security.Claims;
using AuthorizationService.Application.Features.Auth;
using AuthorizationService.Db.Models;
using AuthorizationService.Db.Repositories;

namespace AuthorizationService.Tests.Features.Auth;

public sealed class ValidateAdminQueryHandlerTests
{
    [Fact(DisplayName = "КОГДА claim и роль в БД admin ТОГДА validate admin возвращает userId")]
    public async Task Handle_ReturnsUserId_WhenClaimAndStoredRoleAreAdmin()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateUser(UserRole.Admin);
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        var handler = new ValidateAdminQueryHandler(repository);

        var result = await handler.Handle(
            new ValidateAdminQuery(CreatePrincipal(user.Id, UserRole.Admin)),
            CancellationToken.None);

        Assert.Equal(user.Id, result);
    }

    [Fact(DisplayName = "КОГДА role claim не admin ТОГДА validate admin возвращает null")]
    public async Task Handle_ReturnsNull_WhenRoleClaimIsNotAdmin()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateUser(UserRole.Admin);
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        var handler = new ValidateAdminQueryHandler(repository);

        var result = await handler.Handle(
            new ValidateAdminQuery(CreatePrincipal(user.Id, UserRole.User)),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact(DisplayName = "КОГДА роль в БД не admin ТОГДА validate admin возвращает null")]
    public async Task Handle_ReturnsNull_WhenStoredRoleIsNotAdmin()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateUser(UserRole.User);
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        var handler = new ValidateAdminQueryHandler(repository);

        var result = await handler.Handle(
            new ValidateAdminQuery(CreatePrincipal(user.Id, UserRole.Admin)),
            CancellationToken.None);

        Assert.Null(result);
    }

    private static ClaimsPrincipal CreatePrincipal(Guid userId, UserRole role)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
            },
            authenticationType: "Test"));
    }

    private static User CreateUser(UserRole role)
    {
        var email = $"{Guid.NewGuid():N}@example.com";

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email,
            PasswordHash = "hash",
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
