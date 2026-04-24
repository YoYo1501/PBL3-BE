using BackendAPI.Models.Entities;

namespace BackendAPI.Models.Entities;

public class StudentRequest
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    public string RequestType { get; set; } = string.Empty; // "Checkout", "Maintenance", "Other"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNote { get; set; }
}
