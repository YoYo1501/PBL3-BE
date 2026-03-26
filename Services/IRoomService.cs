using BackendAPI.Models.DTOs.Room;

namespace BackendAPI.Services;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllRooms();
    Task<RoomDto?> GetRoomByIdAsync(int id);
    Task UpdateRoom(BackendAPI.Models.Entities.Room room);
}