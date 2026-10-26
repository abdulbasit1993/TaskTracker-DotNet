using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Api.Models;

public class UpdateTaskRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}