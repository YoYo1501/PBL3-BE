namespace BackendAPI.Models.DTOs.Violation.Responses;

public class StudentViolationInfoDto
{
    public string FullName { get; set; } = string.Empty;
    public string CitizenId { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public int TotalViolations { get; set; }
    public List<ViolationResponseDto> History { get; set; } = [];
}