using AuthorizationService.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationService.Db;

public sealed class AuthorizationDbContext : DbContext
{
    private static readonly Guid AdminUserId = Guid.Parse("8f02d6f7-5c1b-4c4a-9f99-6df3db7bf3c2");
    private static readonly DateTime AdminCreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(user => user.CreatedAtUtc)
                .IsRequired();

            entity.HasData(new User
            {
                Id = AdminUserId,
                Email = "admin@authorization.local",
                PasswordHash = "CHANGE_ME_ADMIN_PASSWORD_HASH",
                Role = UserRole.Admin,
                CreatedAtUtc = AdminCreatedAtUtc,
                UpdatedAtUtc = null
            });
        });
    }
}
