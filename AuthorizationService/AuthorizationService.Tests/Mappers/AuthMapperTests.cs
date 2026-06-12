using System.Security.Claims;
using AuthorizationService.Application.Features.Auth;
using AuthorizationService.Application.Mappers;
using AuthorizationService.Application.Models;
using AuthorizationService.Db.Models;

namespace AuthorizationService.Tests.Mappers;

public sealed class AuthMapperTests
{
    [Fact(DisplayName = "КОГДА маппится LoginRequest ТОГДА создается LoginCommand")]
    public void ToCmd_MapsLoginRequestToLoginCommand()
    {
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "secret123"
        };

        var command = request.ToCmd();

        Assert.Same(request, command.Request);
    }

    [Fact(DisplayName = "КОГДА маппится RegisterRequest ТОГДА создается RegisterCommand")]
    public void ToCmd_MapsRegisterRequestToRegisterCommand()
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "secret123",
            Role = UserRole.Manager
        };

        var command = request.ToCmd();

        Assert.Same(request, command.Request);
    }

    [Fact(DisplayName = "КОГДА ClaimsPrincipal маппится без типа ТОГДА создается ValidateTokenQuery")]
    public void ToQuery_MapsClaimsPrincipalToValidateTokenQueryByDefault()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var query = principal.ToQuery();

        Assert.IsType<ValidateTokenQuery>(query);
    }

    [Fact(DisplayName = "КОГДА ClaimsPrincipal маппится для админа ТОГДА создается ValidateAdminQuery")]
    public void ToQuery_MapsClaimsPrincipalToValidateAdminQuery()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var query = principal.ToQuery(AuthQueryType.ValidateAdmin);

        Assert.IsType<ValidateAdminQuery>(query);
    }

    [Fact(DisplayName = "КОГДА RegisterRequest маппится в entity ТОГДА создается User")]
    public void ToEntity_MapsRegisterRequestToUserEntity()
    {
        var request = new RegisterRequest
        {
            Email = "  User@Example.COM  ",
            Password = "secret123",
            Role = UserRole.Manager
        };

        var user = request.ToEntity("hashed-password");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("User@Example.COM", user.Email);
        Assert.Equal("user@example.com", user.NormalizedEmail);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal(UserRole.Manager, user.Role);
        Assert.True(user.CreatedAtUtc <= DateTime.UtcNow);
    }

    [Fact(DisplayName = "КОГДА User маппится в response ТОГДА создается AuthResponse")]
    public void ToResponse_MapsUserToAuthResponse()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            NormalizedEmail = "user@example.com",
            Role = UserRole.User,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow
        };

        var response = user.ToResponse("token");

        Assert.Equal(user.Id, response.UserId);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(user.Role, response.Role);
        Assert.Equal("token", response.Token);
    }
}
