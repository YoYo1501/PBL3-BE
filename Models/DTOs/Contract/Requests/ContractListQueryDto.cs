using BackendAPI.Models.DTOs.Common;

namespace BackendAPI.Models.DTOs.Contract.Requests;

public class ContractListQueryDto : PagedQueryDto
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}
