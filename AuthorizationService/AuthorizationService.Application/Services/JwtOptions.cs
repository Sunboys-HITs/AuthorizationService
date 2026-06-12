using System.ComponentModel.DataAnnotations;

namespace AuthorizationService.Application.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = "AuthorizationService";

    [Required]
    public string Audience { get; init; } = "AuthorizationService.Clients";

    [Required]
    [MinLength(32)]
    public string Secret { get; init; } = "AuthorizationServiceDevelopmentSecretKey";

    [Range(1, 1440)]
    public int ExpiresMinutes { get; init; } = 60;
}
