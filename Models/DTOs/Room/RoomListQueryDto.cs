using BackendAPI.Models.DTOs.Common;

namespace BackendAPI.Models.DTOs.Room;

public class RoomListQueryDto : PagedQueryDto
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}
