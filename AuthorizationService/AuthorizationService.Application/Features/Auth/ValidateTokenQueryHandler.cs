using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthorizationService.Db.Repositories;
using MediatR;

namespace AuthorizationService.Application.Features.Auth;

public sealed class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, Guid?>
{
    private readonly IUserRepository _userRepository;

    public ValidateTokenQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid?> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserId(request.User);
        if (userId is null)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        return user?.Id;
    }

    internal static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ??
                          user.FindFirst(JwtRegisteredClaimNames.Sub);

        return userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId)
            ? userId
            : null;
    }
}
