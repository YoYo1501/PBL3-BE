using BackendAPI.Models.DTOs.Common;

namespace BackendAPI.Models.DTOs.Revenue.Requests;

public class RevenueFilterDto : PagedQueryDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? RoomCode { get; set; }
    public string? Period { get; set; }
}
