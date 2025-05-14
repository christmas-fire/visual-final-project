using FinalProject.Data;
using FinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Repositories;

public class ArticleRepository : Repository<Article>, IArticleRepository
{
    public ArticleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Article>> GetArticlesWithAuthorsAsync()
    {
        return await _dbSet
            .Include(a => a.Author)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetArticlesByAuthorAsync(int authorId)
    {
        return await _dbSet
            .Include(a => a.Author)
            .Where(a => a.AuthorId == authorId)
            .ToListAsync();
    }
    
    public async Task<Article?> GetArticleWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Author)
            .Include(a => a.Reviews)
            .ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public override async Task<IEnumerable<Article>> GetAllAsync()
    {
         return await _dbSet
            .Include(a => a.Author)
            .ToListAsync();
    }

    public override async Task<Article?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Author) 
            .FirstOrDefaultAsync(a => a.Id == id);
    }
} 