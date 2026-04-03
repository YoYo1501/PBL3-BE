using BackendAPI.Models.DTOs.Room;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;
public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<List<RoomDto>> GetAllRooms()
    {
        var rooms = await _roomRepository.GetAll();
        return rooms.Select(ToDto).ToList();
    }

    public async Task<RoomDto?> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        return room == null ? null : ToDto(room);
    }

    public async Task UpdateRoom(Room room)
        => await _roomRepository.Update(room);

    private static RoomDto ToDto(Room r) => new RoomDto
    {
        Id = r.Id,
        RoomCode = r.RoomCode,
        RoomType = r.RoomType,
        Capacity = r.Capacity,
        CurrentOccupancy = r.CurrentOccupancy,
        Status = r.Status,
        BuildingCode = r.Building.Code,
        BuildingName = r.Building.Name,
        GenderAllowed = r.Building.GenderAllowed
    };
}