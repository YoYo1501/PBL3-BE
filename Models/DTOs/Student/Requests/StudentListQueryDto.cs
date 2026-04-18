using BackendAPI.Models.DTOs.Common;

namespace BackendAPI.Models.DTOs.Student.Requests;

public class StudentListQueryDto : PagedQueryDto
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
}
