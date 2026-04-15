namespace BackendAPI.Models.DTOs.Facility.Requests;

public class CreateFacilityDto
{
    public int RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string Status { get; set; } = "Good";
}

public class UpdateFacilityDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string Status { get; set; } = "Good";
}