namespace BackendAPI.Models.DTOs.Invoice.Requests;

public class InvoiceSettingDto
{
    public string Period { get; set; } = string.Empty;
    public decimal ElectricPricePerKwh { get; set; }
    public decimal WaterPricePerM3 { get; set; }
}