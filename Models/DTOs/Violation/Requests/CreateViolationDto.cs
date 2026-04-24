namespace BackendAPI.Models.DTOs.Violation.Requests;

public class CreateViolationDto
{
    public string CitizenId { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ViolationDate { get; set; }
    public string? Evidence { get; set; }
}