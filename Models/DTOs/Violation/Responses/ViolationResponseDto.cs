namespace BackendAPI.Models.DTOs.Violation.Responses;

public class ViolationResponseDto
{
    public int Id { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ViolationDate { get; set; }
    public string? Evidence { get; set; }
    public int TotalCount { get; set; }
}