using FinalProject.Models;

namespace FinalProject.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetReviewsByReviewerAsync(int reviewerId);
} 