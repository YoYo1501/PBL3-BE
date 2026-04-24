namespace BackendAPI.Models.Entities;

public class Notification : ISoftDelete
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}