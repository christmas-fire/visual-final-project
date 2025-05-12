using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProject.Data;
using FinalProject.Models;
using System.Security.Claims;

namespace FinalProject.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ArticlesController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> GetArticles()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var query = _context.Articles.AsQueryable();

        if (userRole == "Author")
        {
            query = query.Where(a => a.AuthorId == userId);
        }

        var articles = await query
            .Include(a => a.Author)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Status,
                a.SubmissionDate,
                AuthorName = a.Author.Username
            })
            .ToListAsync();

        return Ok(articles);
    }

    [HttpPost]
    [Authorize(Roles = "Author")]
    public async Task<IActionResult> SubmitArticle([FromForm] IFormFile file, [FromForm] string title)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var allowedExtensions = new[] { ".pdf", ".docx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Only PDF and DOCX files are allowed");

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var article = new Article
        {
            Title = title,
            FilePath = fileName,
            SubmissionDate = DateTime.UtcNow,
            Status = "NotReviewed",
            AuthorId = userId
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Article submitted successfully", articleId = article.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticle(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var article = await _context.Articles
            .Include(a => a.Author)
            .Include(a => a.Reviews)
            .ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null)
            return NotFound();

        if (userRole == "Author" && article.AuthorId != userId)
            return Forbid();

        return Ok(new
        {
            article.Id,
            article.Title,
            article.Status,
            article.SubmissionDate,
            AuthorName = article.Author.Username,
            Reviews = article.Reviews.Select(r => new
            {
                r.Id,
                r.Content,
                r.Status,
                r.ReviewDate,
                ReviewerName = r.Reviewer.Username
            })
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
            return NotFound();

        var filePath = Path.Combine(_environment.WebRootPath, "uploads", article.FilePath);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Article deleted successfully" });
    }
} 