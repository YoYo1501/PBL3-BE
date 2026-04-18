using BackendAPI.Models.DTOs.Common;

namespace BackendAPI.Models.DTOs.Invoice.Requests;

public class InvoiceListQueryDto : PagedQueryDto
{
    public string? Period { get; set; }
    public string? Status { get; set; }
}
