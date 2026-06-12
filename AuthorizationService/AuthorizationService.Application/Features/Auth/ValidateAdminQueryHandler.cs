using System.Security.Claims;
using AuthorizationService.Db.Models;
using AuthorizationService.Db.Repositories;
using MediatR;

namespace AuthorizationService.Application.Features.Auth;

public sealed class ValidateAdminQueryHandler : IRequestHandler<ValidateAdminQuery, Guid?>
{
    private readonly IUserRepository _userRepository;

    public ValidateAdminQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid?> Handle(ValidateAdminQuery request, CancellationToken cancellationToken)
    {
        var userId = ValidateTokenQueryHandler.GetUserId(request.User);
        if (userId is null)
        {
            return null;
        }

        var roleClaim = request.User.FindFirst(ClaimTypes.Role);
        if (!string.Equals(roleClaim?.Value, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        return user?.Role == UserRole.Admin ? user.Id : null;
    }
}
