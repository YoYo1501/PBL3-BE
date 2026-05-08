using BackendAPI.Models.DTOs.Common;

namespace BackendAPI.Models.DTOs.Receipt.Requests;

public class ReceiptListQueryDto : PagedQueryDto
{
    public string? Period { get; set; }
    public int? StudentId { get; set; }
}
