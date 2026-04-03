using BackendAPI.Models.DTOs.Room;

namespace BackendAPI.Services.Interfaces;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllRooms();
    Task<RoomDto?> GetRoomByIdAsync(int id);
    Task UpdateRoom(Models.Entities.Room room);
}