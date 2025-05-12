using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models;

public class Review
{
    public int Id { get; set; }
    
    [Required]
    public string Content { get; set; } = null!;
    
    public DateTime ReviewDate { get; set; }
    
    public string Status { get; set; } = null!;
    
    public int ReviewerId { get; set; }
    public User Reviewer { get; set; } = null!;
    
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
} 