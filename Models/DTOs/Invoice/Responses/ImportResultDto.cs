namespace BackendAPI.Models.DTOs.Invoice.Responses;

public class ImportResultDto
{
    public string RoomCode { get; set; } = string.Empty;
    public decimal OldElectric { get; set; }
    public decimal NewElectric { get; set; }
    public decimal OldWater { get; set; }
    public decimal NewWater { get; set; }
    public decimal ElectricUsed => NewElectric - OldElectric;
    public decimal WaterUsed => NewWater - OldWater;
}