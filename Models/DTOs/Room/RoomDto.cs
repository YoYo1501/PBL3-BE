namespace BackendAPI.Models.DTOs.Room;

public class RoomDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int AvailableSlots => Capacity - CurrentOccupancy; // tự tính chỗ trống
    public string Status { get; set; } = string.Empty;
    public string BuildingCode { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string GenderAllowed { get; set; } = string.Empty;
}
