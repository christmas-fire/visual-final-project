using FinalProject.Models;
using FinalProject.Repositories;

namespace FinalProject.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IArticleRepository _articleRepository;

    public ReviewService(IReviewRepository reviewRepository, IArticleRepository articleRepository)
    {
        _reviewRepository = reviewRepository;
        _articleRepository = articleRepository;
    }

    public async Task<IEnumerable<object>> GetMyReviewsAsync(int userId)
    {
        var reviews = await _reviewRepository.GetReviewsByReviewerAsync(userId);
        return reviews.Select(r => new
        {
            r.Id,
            r.Content,
            r.Status,
            r.ReviewDate,
            ArticleTitle = r.Article?.Title,
            ArticleId = r.ArticleId
        });
    }

    public async Task<(bool success, object? result, string? errorMessage)> CreateReviewAsync(CreateReviewDto reviewDto, int userId)
    {
        var article = await _articleRepository.GetByIdAsync(reviewDto.ArticleId);
        if (article == null)
            return (false, null, "Article not found");

        var articleWithReviews = await _articleRepository.GetArticleWithDetailsAsync(reviewDto.ArticleId); 
        if (articleWithReviews?.Reviews.Any(r => r.ReviewerId == userId) == true)
             return (false, null, "You have already reviewed this article");

        var review = new Review
        {
            Content = reviewDto.Content,
            Status = reviewDto.Status,
            ReviewDate = DateTime.UtcNow,
            ReviewerId = userId,
            ArticleId = reviewDto.ArticleId
        };

        article.Status = reviewDto.Status;
        _articleRepository.Update(article); 

        await _reviewRepository.AddAsync(review);
        
        await _articleRepository.SaveChangesAsync();

        return (true, new { message = "Review submitted successfully", reviewId = review.Id }, null);
    }

    public async Task<IEnumerable<object>> GetAvailableArticlesForReviewAsync(int userId)
    {
        var allArticles = await _articleRepository.GetAllAsync(); 
        
        var articlesForReview = allArticles
            .Where(a => a.Status == "NotReviewed" && 
                        (a.Reviews == null || !a.Reviews.Any(r => r.ReviewerId == userId)) &&
                        a.AuthorId != userId)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.SubmissionDate,
                AuthorName = a.Author?.Username
            });

        return articlesForReview;
    }
} 