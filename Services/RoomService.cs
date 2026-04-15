using BackendAPI.Models.DTOs.Room;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;
public class RoomService(IRoomRepository _roomRepository) : IRoomService
{

    public async Task<List<RoomDto>> GetAllRooms()
    {
        var rooms = await _roomRepository.GetAll();
        return rooms.Select(ToDto).ToList();
    }

    public async Task<List<RoomDto>> GetAvailableRooms()
    {
        var rooms = await _roomRepository.GetAll();
        return rooms.Where(r => r.Status == "Available").Select(ToDto).ToList();
    }

    public async Task<RoomDto?> GetRoomByIdAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        return room == null ? null : ToDto(room);
    }

    public async Task<RoomDto?> GetMyRoomAsync(int studentId)
    {
        var room = await _roomRepository.GetRoomByStudentIdAsync(studentId);
        return room == null ? null : ToDto(room);
    }

    public async Task<(bool Success, string Message)> DeleteRoomAsync(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null)
            return (false, "Phòng không tồn tại.");
        if (room.CurrentOccupancy > 0)
            return (false, "Phòng đang có sinh viên ở, không thể xóa.");

        await _roomRepository.DeleteAsync(room);
        return (true, "Xóa phòng thành công.");
    }

    public async Task UpdateRoom(Room room)
        => await _roomRepository.Update(room);

    public async Task<(bool Success, string Message, RoomDto? Data)> CreateRoomAsync(CreateRoomDto dto)
    {
        var buildingExists = await _roomRepository.BuildingExistsAsync(dto.BuildingId);
        if (!buildingExists)
        {
            return (false, "Tòa nhà không tồn tại.", null);
        }

        var existingRooms = await _roomRepository.GetAll();
        if (existingRooms.Any(r => r.RoomCode == dto.RoomCode))
        {
            return (false, "Mã phòng đã tồn tại.", null);
        }

        var newRoom = new Room
        {
            BuildingId = dto.BuildingId,
            RoomCode = dto.RoomCode,
            RoomType = dto.RoomType,
            Capacity = dto.Capacity,
            CurrentOccupancy = dto.CurrentOccupancy,
            Status = dto.Status,
            Price = dto.Price
        };

        await _roomRepository.AddAsync(newRoom);
        
        var createdRoom = await _roomRepository.GetByIdAsync(newRoom.Id);
        return (true, "Tạo phòng thành công.", ToDto(createdRoom!));
    }

    public async Task<(bool Success, string Message)> UpdateRoomAsync(int id, UpdateRoomDto dto)
    {
        var room = await _roomRepository.GetByIdAsync(id);
        if (room == null)
            return (false, "Phòng không tồn tại.");
        
        if (dto.RoomType != null) room.RoomType = dto.RoomType;
        if (dto.Capacity.HasValue) room.Capacity = dto.Capacity.Value;
        if (dto.CurrentOccupancy.HasValue) room.CurrentOccupancy = dto.CurrentOccupancy.Value;
        if (dto.Status != null) room.Status = dto.Status;
        if (dto.Price.HasValue) room.Price = dto.Price.Value;

        await _roomRepository.Update(room);
        return (true, "Cập nhật phòng thành công.");
    }

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