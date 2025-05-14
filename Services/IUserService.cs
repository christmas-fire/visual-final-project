namespace FinalProject.Services;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsBlocked { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<(bool success, UserDto? user, string? errorMessage)> CreateUserAsync(CreateUserDto createUserDto);
    Task<(bool success, string? message, string? errorMessage)> ToggleBlockUserAsync(int userId);
    Task<(bool success, string? errorMessage)> DeleteUserAsync(int userId);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<Models.User?> GetUserByUsernameAsync(string username);
} 