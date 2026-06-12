using System.IdentityModel.Tokens.Jwt;
using AuthorizationService.Application.Services;
using AuthorizationService.Db.Models;
using Microsoft.Extensions.Options;

namespace AuthorizationService.Tests.Services;

public sealed class JwtTokenGeneratorTests
{
    [Fact(DisplayName = "КОГДА генерируется JWT ТОГДА токен содержит ожидаемые claims")]
    public void Generate_CreatesTokenWithExpectedClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            NormalizedEmail = "admin@example.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            CreatedAtUtc = DateTime.UtcNow
        };
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Secret = "test-secret-value-longer-than-32-chars",
            ExpiresMinutes = 30
        });
        var generator = new JwtTokenGenerator(options);

        var token = generator.Generate(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == "email" && claim.Value == user.Email);
        Assert.Contains(jwt.Claims, claim => claim.Type.EndsWith("/role", StringComparison.Ordinal) && claim.Value == "Admin");
    }
}
