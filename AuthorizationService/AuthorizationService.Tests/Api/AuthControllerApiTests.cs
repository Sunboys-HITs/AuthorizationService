using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AuthorizationService.Application.Models;
using AuthorizationService.Db.Models;

namespace AuthorizationService.Tests.Api;

public sealed class AuthControllerApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact(DisplayName = "КОГДА вызывается health endpoint ТОГДА возвращается Healthy")]
    public async Task Health_ReturnsHealthy()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/health");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory(DisplayName = "КОГДА вызывается infrastructure health endpoint ТОГДА возвращается Success")]
    [InlineData("/api/auth/health/live")]
    [InlineData("/api/auth/health/ready")]
    public async Task HealthChecks_ReturnSuccess(string url)
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact(DisplayName = "КОГДА регистрация валидна ТОГДА API возвращает Created")]
    public async Task Register_ReturnsCreated_WhenRequestIsValid()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        var request = CreateRegisterRequest("user@example.com", UserRole.User);

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        await AssertStatusCodeAsync(HttpStatusCode.Created, response);
    }

    [Fact(DisplayName = "КОГДА регистрация невалидна ТОГДА API возвращает BadRequest")]
    public async Task Register_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        var request = new RegisterRequest
        {
            Email = "not-email",
            Password = "123",
            Role = UserRole.User
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        await AssertStatusCodeAsync(HttpStatusCode.BadRequest, response);
    }

    [Fact(DisplayName = "КОГДА email уже зарегистрирован ТОГДА API возвращает Conflict")]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        var request = CreateRegisterRequest("user@example.com", UserRole.User);
        await client.PostAsJsonAsync("/api/auth/register", request);

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        await AssertStatusCodeAsync(HttpStatusCode.Conflict, response);
    }

    [Fact(DisplayName = "КОГДА логин валиден ТОГДА API возвращает AuthResponse")]
    public async Task Login_ReturnsAuthResponse_WhenCredentialsAreValid()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        await RegisterAsync(client, "user@example.com", UserRole.User);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "user@example.com",
            Password = "secret123"
        });

        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(authResponse);
        Assert.Equal("user@example.com", authResponse.Email);
        Assert.Equal(UserRole.User, authResponse.Role);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.Token));
    }

    [Fact(DisplayName = "КОГДА пользователь не найден ТОГДА login API возвращает NotFound")]
    public async Task Login_ReturnsNotFound_WhenUserDoesNotExist()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "missing@example.com",
            Password = "secret123"
        });

        await AssertStatusCodeAsync(HttpStatusCode.NotFound, response);
    }

    [Fact(DisplayName = "КОГДА пароль неверный ТОГДА login API возвращает Unauthorized")]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        await RegisterAsync(client, "user@example.com", UserRole.User);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "user@example.com",
            Password = "wrong-password"
        });

        await AssertStatusCodeAsync(HttpStatusCode.Unauthorized, response);
    }

    [Fact(DisplayName = "КОГДА validate вызывается без токена ТОГДА API возвращает Unauthorized")]
    public async Task Validate_ReturnsUnauthorized_WhenTokenIsMissing()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/validate");

        await AssertStatusCodeAsync(HttpStatusCode.Unauthorized, response);
    }

    [Fact(DisplayName = "КОГДА validate вызывается с токеном ТОГДА API возвращает OK и X-User-Id")]
    public async Task Validate_ReturnsOkAndUserIdHeader_WhenTokenIsValid()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        var authResponse = await RegisterAndLoginAsync(client, "user@example.com", UserRole.User);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

        var response = await client.GetAsync("/api/auth/validate");

        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues("X-User-Id", out var values));
        Assert.Contains(authResponse.UserId.ToString(), values);
    }

    [Fact(DisplayName = "КОГДА validate admin вызывается пользователем ТОГДА API возвращает Forbidden")]
    public async Task ValidateAdmin_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        var authResponse = await RegisterAndLoginAsync(client, "user@example.com", UserRole.User);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

        var response = await client.GetAsync("/api/auth/validate_admin");

        await AssertStatusCodeAsync(HttpStatusCode.Forbidden, response);
    }

    [Fact(DisplayName = "КОГДА validate admin вызывается админом ТОГДА API возвращает OK и X-User-Id")]
    public async Task ValidateAdmin_ReturnsOkAndUserIdHeader_WhenUserIsAdmin()
    {
        await using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        var authResponse = await RegisterAndLoginAsync(client, "admin@example.com", UserRole.Admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

        var response = await client.GetAsync("/api/auth/validate_admin");

        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues("X-User-Id", out var values));
        Assert.Contains(authResponse.UserId.ToString(), values);
    }

    private static RegisterRequest CreateRegisterRequest(string email, UserRole role)
    {
        return new RegisterRequest
        {
            Email = email,
            Password = "secret123",
            Role = role
        };
    }

    private static async Task RegisterAsync(HttpClient client, string email, UserRole role)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", CreateRegisterRequest(email, role));
        await AssertSuccessAsync(response);
    }

    private static async Task<AuthResponse> RegisterAndLoginAsync(HttpClient client, string email, UserRole role)
    {
        await RegisterAsync(client, email, role);
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "secret123"
        });
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions) ??
               throw new InvalidOperationException("Auth response was empty.");
    }

    private static async Task AssertSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected success status code, got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }
    }

    private static async Task AssertStatusCodeAsync(HttpStatusCode expected, HttpResponseMessage response)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected {(int)expected} {expected}, got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }
    }
}
