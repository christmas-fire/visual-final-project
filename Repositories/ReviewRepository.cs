using FinalProject.Data;
using FinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Review>> GetReviewsByReviewerAsync(int reviewerId)
    {
        return await _dbSet
            .Include(r => r.Article)
            .Where(r => r.ReviewerId == reviewerId)
            .ToListAsync();
    }
} 