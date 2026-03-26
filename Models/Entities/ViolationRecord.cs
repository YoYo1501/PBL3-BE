namespace BackendAPI.Models.Entities;

public class ViolationRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ViolationDate { get; set; }
    public string? Evidence { get; set; }
    public int TotalCount { get; set; } = 1;

    public Student Student { get; set; } = null!;
}