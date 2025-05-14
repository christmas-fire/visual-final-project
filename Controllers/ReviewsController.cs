using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinalProject.Services;

namespace FinalProject.Controllers;

[Authorize(Roles = "Reviewer")]
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var reviews = await _reviewService.GetMyReviewsAsync(userId);
        return Ok(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var (success, result, errorMessage) = await _reviewService.CreateReviewAsync(request, userId);

        if (!success)
        {
            if (errorMessage == "Article not found")
                return NotFound(errorMessage);
            return BadRequest(errorMessage ?? "Failed to create review.");
        }
        return Ok(result);
    }

    [HttpGet("available-articles")]
    public async Task<IActionResult> GetAvailableArticles()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var articles = await _reviewService.GetAvailableArticlesForReviewAsync(userId);
        return Ok(articles);
    }
} 