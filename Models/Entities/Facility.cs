namespace BackendAPI.Models.Entities;

public class Facility : ISoftDelete
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty; // Tên thi?t b? (Gi??ng, Qu?t, ?i?u hòa...)
    public int Quantity { get; set; } = 1;
    public string Status { get; set; } = "Good"; // Good, Damaged, UnderMaintenance
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}