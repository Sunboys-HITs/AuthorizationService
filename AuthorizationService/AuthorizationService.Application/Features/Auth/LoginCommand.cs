using AuthorizationService.Application.Models;
using MediatR;

namespace AuthorizationService.Application.Features.Auth;

public sealed record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
