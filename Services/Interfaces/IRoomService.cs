using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Room;

namespace BackendAPI.Services.Interfaces;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllRooms();
    Task<PagedResultDto<RoomDto>> GetPagedRoomsAsync(RoomListQueryDto query);
    Task<List<RoomDto>> GetAvailableRooms();
    Task<RoomDto?> GetRoomByIdAsync(int id);
    Task<RoomDto?> GetMyRoomAsync(int studentId);
    Task<(bool Success, string Message, RoomDto? Data)> CreateRoomAsync(CreateRoomDto dto);
    Task<(bool Success, string Message)> UpdateRoomAsync(int id, UpdateRoomDto dto);
    Task UpdateRoom(Models.Entities.Room room);
    Task<(bool Success, string Message)> DeleteRoomAsync(int id);
}
