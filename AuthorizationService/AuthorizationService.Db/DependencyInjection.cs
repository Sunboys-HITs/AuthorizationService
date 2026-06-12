using AuthorizationService.Db.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthorizationService.Db;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorizationDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthorizationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AuthorizationDb")));
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
