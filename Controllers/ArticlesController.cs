using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinalProject.Services;

namespace FinalProject.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly IWebHostEnvironment _environment;

    public ArticlesController(IArticleService articleService, IWebHostEnvironment environment)
    {
        _articleService = articleService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> GetArticles()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var articles = await _articleService.GetArticlesAsync(userId, userRole);
        return Ok(articles);
    }

    [HttpPost]
    [Authorize(Roles = "Author")]
    public async Task<IActionResult> SubmitArticle([FromForm] IFormFile file, [FromForm] string title)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var webRootPath = _environment.WebRootPath;

        var (success, result, errorMessage) = await _articleService.SubmitArticleAsync(file, title, userId, webRootPath);

        if (!success)
        {
            return BadRequest(errorMessage ?? "Failed to submit article.");
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticle(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        try
        {
            var article = await _articleService.GetArticleByIdAsync(id, userId, userRole);
            if (article == null)
                return NotFound();

            return Ok(article);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        var webRootPath = _environment.WebRootPath;
        var (success, errorMessage) = await _articleService.DeleteArticleAsync(id, webRootPath);

        if (!success)
        {
            if (errorMessage == "Article not found")
                return NotFound();
            return BadRequest(errorMessage ?? "Failed to delete article.");
        }
        return Ok(new { message = "Article deleted successfully" });
    }
} 