using AuthorizationService.Db.Models;

namespace AuthorizationService.Application.Models;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    UserRole Role,
    string Token);
