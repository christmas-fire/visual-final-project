using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models;

public class Article
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;
    
    [Required]
    public string FilePath { get; set; } = null!;
    
    public DateTime SubmissionDate { get; set; }
    
    public string Status { get; set; } = "NotReviewed";
    
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    
    public List<Review> Reviews { get; set; } = new();
} 