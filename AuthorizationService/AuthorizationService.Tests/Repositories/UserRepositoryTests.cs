using AuthorizationService.Db.Models;
using AuthorizationService.Db.Repositories;

namespace AuthorizationService.Tests.Repositories;

public sealed class UserRepositoryTests
{
    [Fact(DisplayName = "КОГДА пользователь ищется по email ТОГДА используется normalized email")]
    public async Task GetByEmailAsync_ReturnsUser_ByNormalizedEmail()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateUser("User@Example.COM");
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var result = await repository.GetByEmailAsync(" user@example.com ", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact(DisplayName = "КОГДА пользователь существует ТОГДА ExistsByEmailAsync возвращает true")]
    public async Task ExistsByEmailAsync_ReturnsTrue_WhenUserExists()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);
        await repository.AddAsync(CreateUser("user@example.com"), CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var exists = await repository.ExistsByEmailAsync("USER@example.com", CancellationToken.None);

        Assert.True(exists);
    }

    [Fact(DisplayName = "КОГДА пользователь по id не найден ТОГДА возвращается null")]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        await using var dbContext = TestDbFactory.CreateDbContext();
        var repository = new UserRepository(dbContext);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    private static User CreateUser(string email)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.Trim().ToLowerInvariant(),
            PasswordHash = "hash",
            Role = UserRole.User,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
