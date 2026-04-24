namespace BackendAPI.Models.DTOs.Facility.Responses;

public class FacilityResponseDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}