using System.Security.Claims;
using MediatR;

namespace AuthorizationService.Application.Features.Auth;

public sealed record ValidateTokenQuery(ClaimsPrincipal User) : IRequest<Guid?>;
