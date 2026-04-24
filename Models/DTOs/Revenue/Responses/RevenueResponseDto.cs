using BackendAPI.Models.DTOs.Common;

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
    public PagedResultDto<RevenueDetailDto> Details { get; set; } = new();
}
