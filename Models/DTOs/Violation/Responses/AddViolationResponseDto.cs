namespace BackendAPI.Models.DTOs.Violation.Responses;

public class AddViolationResponseDto
{
    public string StudentName { get; set; } = string.Empty;
    public int TotalViolations { get; set; }
    public string HandleResult { get; set; } = string.Empty;
}