using FinalProject.Models;
using FinalProject.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinalProject.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserService userService, IUserRepository userRepository, IConfiguration configuration)
    {
        _userService = userService;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<(bool success, RegisterResponseDto? result, string? errorMessage)> RegisterAsync(RegisterDto registerDto)
    {
        if (await _userRepository.IsEmailTakenAsync(registerDto.Email))
            return (false, null, "Email already registered");

        if (await _userRepository.IsUsernameTakenAsync(registerDto.Username))
            return (false, null, "Username already taken");

        var isFirstUser = !(await _userRepository.GetAllAsync()).Any();

        var userRole = isFirstUser ? "Admin" : "Author";

        var userToCreate = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = userRole,
            IsBlocked = false
        };

        await _userRepository.AddAsync(userToCreate);
        await _userRepository.SaveChangesAsync();
        
        var response = new RegisterResponseDto
        {
            Message = "Registration successful",
            Role = userToCreate.Role,
            IsFirstUser = isFirstUser
        };

        return (true, response, null);
    }

    public async Task<(bool success, AuthResponseDto? result, string? errorMessage)> LoginAsync(LoginDto loginDto)
    {
        Models.User? user = null;

        if (!string.IsNullOrWhiteSpace(loginDto.Email) && loginDto.Email.Contains("@"))
        {
            user = await _userRepository.GetByEmailAsync(loginDto.Email);
        }

        if (user == null && !string.IsNullOrWhiteSpace(loginDto.Email))
        {
            user = await _userRepository.GetByUsernameAsync(loginDto.Email); 
        }

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            return (false, null, "Invalid email/username or password");

        if (user.IsBlocked)
            return (false, null, "Account is blocked");

        var token = GenerateJwtToken(user);
        var userDto = new UserDto { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role, IsBlocked = user.IsBlocked };
        
        var authResponse = new AuthResponseDto
        {
            Token = token,
            User = userDto
        };

        return (true, authResponse, null);
    }

    private string GenerateJwtToken(User user)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured."));
        var key = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = credentials
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
} 