using AuthorizationService.Application.Mappers;
using AuthorizationService.Application.Services;
using AuthorizationService.Db.Repositories;
using MediatR;

namespace AuthorizationService.Application.Features.Auth;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Request.Email, cancellationToken))
        {
            throw new ArgumentException("User with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Request.Password);
        var user = request.Request.ToEntity(passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
