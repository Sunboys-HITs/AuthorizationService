using AuthorizationService.AppHost.Models;
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
       
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        
    }

    [HttpGet("validate")]
    [Authorize]
    public async Task<IActionResult> Validate()
    {
        
    }

    [HttpGet("validate_admin")]
    [Authorize]
    public async Task<IActionResult> ValidateAdmin()
    {
        
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
