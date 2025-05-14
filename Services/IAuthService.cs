namespace FinalProject.Services;

public class RegisterDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

public class RegisterResponseDto
{
    public string Message { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsFirstUser { get; set; }
}

public interface IAuthService
{
    Task<(bool success, RegisterResponseDto? result, string? errorMessage)> RegisterAsync(RegisterDto registerDto);
    Task<(bool success, AuthResponseDto? result, string? errorMessage)> LoginAsync(LoginDto loginDto);
} 