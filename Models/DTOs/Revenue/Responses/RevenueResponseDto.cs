namespace BackendAPI.Models.DTOs.Revenue.Responses;

public class RevenueResponseDto
{
    public decimal TotalRoomFee { get; set; }
    public decimal TotalElectricFee { get; set; }
    public decimal TotalWaterFee { get; set; }
    public decimal GrandTotal { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public List<RevenueDetailDto> Details { get; set; } = [];
}