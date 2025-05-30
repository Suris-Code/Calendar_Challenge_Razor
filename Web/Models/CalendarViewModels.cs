using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class CalendarEventViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsCancelled { get; set; }
    public string? Email { get; set; }
}

public class CreateEventViewModel
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Start { get; set; } = string.Empty;
    
    [Required]
    public string End { get; set; } = string.Empty;
    
    public string? Location { get; set; }
}

public class UpdateEventViewModel
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Start { get; set; } = string.Empty;
    
    [Required]
    public string End { get; set; } = string.Empty;
    
    [Required]
    public string? Location { get; set; }
} 