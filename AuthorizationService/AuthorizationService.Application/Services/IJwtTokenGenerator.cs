using AuthorizationService.Db.Models;

namespace AuthorizationService.Application.Services;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
