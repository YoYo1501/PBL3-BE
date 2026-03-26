namespace BackendAPI.Models.DTOs.Registration;

public class RegistrationResponseDto
{
    public int Id { get; set; }
    public string RegistrationCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmittedAt { get; set; }
}