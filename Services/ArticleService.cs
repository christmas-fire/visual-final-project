using FinalProject.Models;
using FinalProject.Repositories;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace FinalProject.Services;

public class ArticleService : IArticleService
{
    private readonly IArticleRepository _articleRepository;

    public ArticleService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }

    public async Task<IEnumerable<object>> GetArticlesAsync(int userId, string? userRole)
    {
        IEnumerable<Article> articles;
        if (userRole == "Author")
        {
            articles = await _articleRepository.GetArticlesByAuthorAsync(userId);
        }
        else
        {
            articles = await _articleRepository.GetArticlesWithAuthorsAsync();
        }

        return articles.Select(a => new
        {
            a.Id,
            a.Title,
            a.Status,
            a.SubmissionDate,
            AuthorName = a.Author?.Username
        });
    }

    public async Task<object?> GetArticleByIdAsync(int id, int userId, string? userRole)
    {
        var article = await _articleRepository.GetArticleWithDetailsAsync(id);

        if (article == null)
            return null;

        if (userRole == "Author" && article.AuthorId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view this article.");

        return new
        {
            article.Id,
            article.Title,
            article.Status,
            article.SubmissionDate,
            AuthorName = article.Author?.Username,
            FilePath = article.FilePath,
            Reviews = article.Reviews?.Select(r => new
            {
                r.Id,
                r.Content,
                r.Status,
                r.ReviewDate,
                ReviewerName = r.Reviewer?.Username
            }) ?? Enumerable.Empty<object>()
        };
    }
    
    public async Task<(bool success, object? result, string? errorMessage)> SubmitArticleAsync(IFormFile file, string title, int userId, string webRootPath)
    {
        if (file == null || file.Length == 0)
            return (false, null, "No file uploaded");

        var allowedExtensions = new[] { ".pdf", ".docx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
             return (false, null, "Only PDF and DOCX files are allowed");

        var uploadsFolder = Path.Combine(webRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        try
        {
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

            await _articleRepository.AddAsync(article);
            await _articleRepository.SaveChangesAsync();

            return (true, new { message = "Article submitted successfully", articleId = article.Id }, null);
        }
        catch (Exception ex)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            return (false, null, $"An error occurred while saving the article: {ex.Message}");
        }
    }

    public async Task<(bool success, string? errorMessage)> DeleteArticleAsync(int id, string webRootPath)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null)
            return (false, "Article not found");

        var filePath = Path.Combine(webRootPath, "uploads", article.FilePath);
        if (System.IO.File.Exists(filePath))
        {
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
            }
        }

        _articleRepository.Remove(article);
        await _articleRepository.SaveChangesAsync();

        return (true, null);
    }
} 