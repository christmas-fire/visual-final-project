using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProject.Data;
using FinalProject.Models;
using System.Security.Claims;

namespace FinalProject.Controllers;

[Authorize(Roles = "Reviewer")]
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var reviews = await _context.Reviews
            .Include(r => r.Article)
            .Where(r => r.ReviewerId == userId)
            .Select(r => new
            {
                r.Id,
                r.Content,
                r.Status,
                r.ReviewDate,
                ArticleTitle = r.Article.Title,
                ArticleId = r.Article.Id
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var article = await _context.Articles
            .Include(a => a.Reviews)
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId);

        if (article == null)
            return NotFound("Article not found");

        if (article.Reviews.Any(r => r.ReviewerId == userId))
            return BadRequest("You have already reviewed this article");

        var review = new Review
        {
            Content = request.Content,
            Status = request.Status,
            ReviewDate = DateTime.UtcNow,
            ReviewerId = userId,
            ArticleId = request.ArticleId
        };

        article.Status = request.Status;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Review submitted successfully", reviewId = review.Id });
    }

    [HttpGet("available-articles")]
    public async Task<IActionResult> GetAvailableArticles()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var articles = await _context.Articles
            .Include(a => a.Reviews)
            .Where(a => a.Status == "NotReviewed" && !a.Reviews.Any(r => r.ReviewerId == userId))
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.SubmissionDate,
                AuthorName = a.Author.Username
            })
            .ToListAsync();

        return Ok(articles);
    }
}

public class CreateReviewRequest
{
    public int ArticleId { get; set; }
    public string Content { get; set; } = null!;
    public string Status { get; set; } = null!;
} 