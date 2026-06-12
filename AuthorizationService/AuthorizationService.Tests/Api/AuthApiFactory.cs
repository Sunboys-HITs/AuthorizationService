using AuthorizationService.Db;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AuthorizationService.Tests.Api;

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AuthorizationDb"] = "Host=localhost;Database=test",
                ["Jwt:Issuer"] = "AuthorizationService",
                ["Jwt:Audience"] = "AuthorizationService.Clients",
                ["Jwt:Secret"] = "AuthorizationServiceDevelopmentSecretKey",
                ["Jwt:ExpiresMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AuthorizationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AuthorizationDbContext>>();
            services.AddDbContext<AuthorizationDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
