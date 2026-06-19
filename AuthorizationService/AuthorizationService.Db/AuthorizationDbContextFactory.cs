using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthorizationService.Db;

public sealed class AuthorizationDbContextFactory : IDesignTimeDbContextFactory<AuthorizationDbContext>
{
    public AuthorizationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthorizationDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=authorization_service;Username=postgres;Password=postgres")
            .Options;

        return new AuthorizationDbContext(options);
    }
}
