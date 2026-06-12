using AuthorizationService.Application.Services;

namespace AuthorizationService.Tests.Services;

public sealed class Pbkdf2PasswordHasherTests
{
    [Fact(DisplayName = "КОГДА пароль соответствует хешу ТОГДА Verify возвращает true")]
    public void Verify_ReturnsTrue_ForPasswordMatchingHash()
    {
        var hasher = new Pbkdf2PasswordHasher();

        var hash = hasher.Hash("secret123");

        Assert.True(hasher.Verify("secret123", hash));
    }

    [Fact(DisplayName = "КОГДА пароль не соответствует хешу ТОГДА Verify возвращает false")]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("secret123");

        Assert.False(hasher.Verify("another-password", hash));
    }

    [Theory(DisplayName = "КОГДА хеш невалидный ТОГДА Verify возвращает false")]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("100000.not-base64.not-base64")]
    public void Verify_ReturnsFalse_ForInvalidHash(string invalidHash)
    {
        var hasher = new Pbkdf2PasswordHasher();

        Assert.False(hasher.Verify("secret123", invalidHash));
    }
}
