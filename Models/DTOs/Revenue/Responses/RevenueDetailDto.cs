namespace BackendAPI.Models.DTOs.Revenue.Responses;

public class RevenueDetailDto
{
    public string Period { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal RoomFee { get; set; }
    public decimal ElectricFee { get; set; }
    public decimal WaterFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
}