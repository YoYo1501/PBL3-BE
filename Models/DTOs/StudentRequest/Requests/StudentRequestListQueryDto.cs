using BackendAPI.Models.DTOs.Common;

namespace BackendAPI.Models.DTOs.StudentRequest.Requests;

public class StudentRequestListQueryDto : PagedQueryDto
{
    public string? Status { get; set; }
    public string? RequestType { get; set; }
}
