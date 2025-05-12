using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string Email { get; set; } = null!;
    
    [Required]
    public string PasswordHash { get; set; } = null!;
    
    [Required]
    public string Role { get; set; } = null!;
    
    public bool IsBlocked { get; set; }
    
    public List<Article> Articles { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
} 