using AuthorizationService.Db;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationService.Tests;

internal static class TestDbFactory
{
    public static AuthorizationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthorizationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthorizationDbContext(options);
    }
}
