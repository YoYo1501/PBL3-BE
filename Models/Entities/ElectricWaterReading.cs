namespace BackendAPI.Models.Entities;

public class ElectricWaterReading
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string Period { get; set; } = string.Empty; // "03/2026"
    public decimal OldElectric { get; set; }
    public decimal NewElectric { get; set; }
    public decimal OldWater { get; set; }
    public decimal NewWater { get; set; }

    // Navigation
    public Room Room { get; set; } = null!;
}