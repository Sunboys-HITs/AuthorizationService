using AuthorizationService.Application.Mappers;
using AuthorizationService.Application.Models;
using AuthorizationService.Application.Services;
using AuthorizationService.Db.Repositories;
using MediatR;

namespace AuthorizationService.Application.Features.Auth;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Request.Email, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        if (!_passwordHasher.Verify(request.Request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid password.");
        }

        return user.ToResponse(_jwtTokenGenerator.Generate(user));
    }
}
