using System.ComponentModel.DataAnnotations;
using AuthorizationService.Db.Models;

namespace AuthorizationService.Application.Models;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; } = UserRole.User;
}
