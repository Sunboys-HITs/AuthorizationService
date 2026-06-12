using System.Security.Claims;
using MediatR;

namespace AuthorizationService.Application.Features.Auth;

public sealed record ValidateAdminQuery(ClaimsPrincipal User) : IRequest<Guid?>;
