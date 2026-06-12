using System.Security.Claims;
using AuthorizationService.Application.Features.Auth;
using AuthorizationService.Application.Models;
using AuthorizationService.Db.Models;
using MediatR;

namespace AuthorizationService.Application.Mappers;

public enum AuthQueryType
{
    Validate,
    ValidateAdmin
}

public static class AuthMapper
{
    public static LoginCommand ToCmd(this LoginRequest request)
    {
        return new LoginCommand(request);
    }

    public static RegisterCommand ToCmd(this RegisterRequest request)
    {
        return new RegisterCommand(request);
    }

    public static IRequest<Guid?> ToQuery(this ClaimsPrincipal user, AuthQueryType queryType = AuthQueryType.Validate)
    {
        return queryType switch
        {
            AuthQueryType.ValidateAdmin => new ValidateAdminQuery(user),
            _ => new ValidateTokenQuery(user)
        };
    }

    public static User ToEntity(this RegisterRequest request, string passwordHash)
    {
        var email = request.Email.Trim();

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = request.Role,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static AuthResponse ToResponse(this User user, string token)
    {
        return new AuthResponse(user.Id, user.Email, user.Role, token);
    }
}
