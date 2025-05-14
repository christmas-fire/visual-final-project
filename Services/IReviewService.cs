using FinalProject.Models;
using FinalProject.Controllers;

namespace FinalProject.Services;

public class CreateReviewDto
{
    public int ArticleId { get; set; }
    public string Content { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public interface IReviewService
{
    Task<IEnumerable<object>> GetMyReviewsAsync(int userId);
    Task<(bool success, object? result, string? errorMessage)> CreateReviewAsync(CreateReviewDto reviewDto, int userId);
    Task<IEnumerable<object>> GetAvailableArticlesForReviewAsync(int userId);
} 