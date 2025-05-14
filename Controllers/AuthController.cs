using Microsoft.AspNetCore.Mvc;
using FinalProject.Services;

namespace FinalProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var (success, result, errorMessage) = await _authService.RegisterAsync(request);
        if (!success)
        {
            return BadRequest(errorMessage ?? "Registration failed.");
        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var (success, result, errorMessage) = await _authService.LoginAsync(request);
        if (!success)
        {
            if (errorMessage == "Invalid email/username or password" || errorMessage == "Account is blocked")
                return Unauthorized(errorMessage);
            return BadRequest(errorMessage ?? "Login failed.");
        }
        return Ok(result);
    }
} 