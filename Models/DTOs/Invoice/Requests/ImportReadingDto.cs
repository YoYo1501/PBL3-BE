namespace BackendAPI.Models.DTOs.Invoice.Requests;

public class ImportReadingDto
{
    public string RoomCode { get; set; } = string.Empty;
    public decimal OldElectric { get; set; }
    public decimal NewElectric { get; set; }
    public decimal OldWater { get; set; }
    public decimal NewWater { get; set; }
}