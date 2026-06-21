using AuthorizationService.Application.Features.Auth;
using AuthorizationService.Application.Mappers;
using AuthorizationService.Application.Models;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationService.AppHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("health")]
    public Task<IActionResult> HealthCheck()
    {
        _logger.LogInformation("Health check endpoint called.");
        return Task.FromResult<IActionResult>(Ok(new { status = "Healthy" }));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login request called. Email: {Email}", request.Email);
        if (!ModelState.IsValid)
        {
            return ValidationFailed();
        }

        try
        {
            var response = await _mediator.Send(request.ToCmd());
            return Ok(response);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "Login failed because user was not found.");
            return NotFound(e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogWarning(e, "Login failed because credentials are invalid.");
            return Unauthorized(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Login failed.");
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Register request called. Email: {Email}", request.Email);
        if (!ModelState.IsValid)
        {
            return ValidationFailed();
        }

        try
        {
            await _mediator.Send(request.ToCmd());
            return StatusCode(201);
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Registration failed.");
            return Conflict(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Registration failed.");
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("validate")]
    [Authorize]
    public async Task<IActionResult> Validate()
    {
        var userId = await _mediator.Send(User.ToQuery());
        if (userId is null)
        {
            return Unauthorized();
        }

        var userRole = ValidateTokenQueryHandler.GetUserRole(User);
        if (userRole is null)
        {
            return Unauthorized();
        }

        Response.Headers.Append("X-User-Id", userId.Value.ToString());
        Response.Headers.Append("X-User-Role", userRole.Value.ToString());
        return Ok();
    }

    [HttpGet("validate_admin")]
    [Authorize]
    public async Task<IActionResult> ValidateAdmin()
    {
        var tokenUserId = await _mediator.Send(User.ToQuery());
        if (tokenUserId is null)
        {
            return Unauthorized();
        }

        var adminUserId = await _mediator.Send(User.ToQuery(AuthQueryType.ValidateAdmin));
        if (adminUserId is null)
        {
            return Forbid();
        }

        Response.Headers.Append("X-User-Id", adminUserId.Value.ToString());
        return Ok();
    }

    private BadRequestObjectResult ValidationFailed()
    {
        return BadRequest(new
        {
            success = false,
            message = "Validation failed",
            errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? [])
        });
    }
}
