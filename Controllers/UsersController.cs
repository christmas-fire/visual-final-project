using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinalProject.Services;

namespace FinalProject.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetUsersAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        var (success, user, errorMessage) = await _userService.CreateUserAsync(request);

        if (!success)
        {
            return BadRequest(errorMessage ?? "Failed to create user.");
        }
        return Ok(new { message = "User created successfully", userId = user?.Id, user });
    }

    [HttpPut("{id}/block")]
    public async Task<IActionResult> ToggleBlockUser(int id)
    {
        var (success, message, errorMessage) = await _userService.ToggleBlockUserAsync(id);
        if (!success)
        {
            if (errorMessage == "User not found")
                return NotFound(errorMessage);
            return BadRequest(errorMessage ?? "Operation failed.");
        }
        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var (success, errorMessage) = await _userService.DeleteUserAsync(id);
        if (!success)
        {
            if (errorMessage == "User not found")
                return NotFound(errorMessage);
            return BadRequest(errorMessage ?? "Failed to delete user.");
        }
        return Ok(new { message = "User deleted successfully" });
    }
}