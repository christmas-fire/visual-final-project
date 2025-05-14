using FinalProject.Models;
using FinalProject.Repositories;

namespace FinalProject.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role,
            IsBlocked = u.IsBlocked
        });
    }

    public async Task<(bool success, UserDto? user, string? errorMessage)> CreateUserAsync(CreateUserDto createUserDto)
    {
        if (await _userRepository.IsEmailTakenAsync(createUserDto.Email))
            return (false, null, "Email already registered");

        if (await _userRepository.IsUsernameTakenAsync(createUserDto.Username))
            return (false, null, "Username already taken");

        if (createUserDto.Role != "Author" && createUserDto.Role != "Reviewer" && createUserDto.Role != "Admin")
            return (false, null, "Invalid role specified. Allowed roles are Author, Reviewer, Admin.");

        var user = new User
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            Role = createUserDto.Role,
            IsBlocked = false
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsBlocked = user.IsBlocked
        };
        return (true, userDto, null);
    }

    public async Task<(bool success, string? message, string? errorMessage)> ToggleBlockUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return (false, null, "User not found");

        user.IsBlocked = !user.IsBlocked;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return (true, $"User {(user.IsBlocked ? "blocked" : "unblocked")} successfully", null);
    }

    public async Task<(bool success, string? errorMessage)> DeleteUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found");

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsBlocked = user.IsBlocked
        };
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }
} 