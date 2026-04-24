using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Room;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class RoomService(IRoomRepository roomRepository) : IRoomService
{
    public async Task<List<RoomDto>> GetAllRooms()
    {
        var rooms = await roomRepository.GetAll();
        return rooms.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<RoomDto>> GetPagedRoomsAsync(RoomListQueryDto query)
    {
        var page = query.GetPage();
        var pageSize = query.GetPageSize(8);
        var (items, totalCount) = await roomRepository.GetPagedAsync(query.Keyword, query.Status, page, pageSize);

        return new PagedResultDto<RoomDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };
    }

    public async Task<List<RoomDto>> GetAvailableRooms()
    {
        var rooms = await roomRepository.GetAll();
        return rooms.Where(r => r.Status == "Available").Select(ToDto).ToList();
    }

    public async Task<RoomDto?> GetRoomByIdAsync(int id)
    {
        var room = await roomRepository.GetByIdAsync(id);
        return room == null ? null : ToDto(room);
    }

    public async Task<RoomDto?> GetMyRoomAsync(int studentId)
    {
        var room = await roomRepository.GetRoomByStudentIdAsync(studentId);
        return room == null ? null : ToDto(room);
    }

    public async Task<(bool Success, string Message)> DeleteRoomAsync(int id)
    {
        var room = await roomRepository.GetByIdAsync(id);
        if (room == null)
            return (false, "Phòng không tồn tại.");
        if (room.CurrentOccupancy > 0)
            return (false, "Phòng đang có sinh viên ở, không thể xóa.");

        await roomRepository.DeleteAsync(room);
        return (true, "Xóa phòng thành công.");
    }

    public async Task UpdateRoom(Room room)
        => await roomRepository.Update(room);

    public async Task<(bool Success, string Message, RoomDto? Data)> CreateRoomAsync(CreateRoomDto dto)
    {
        var buildingExists = await roomRepository.BuildingExistsAsync(dto.BuildingId);
        if (!buildingExists)
        {
            return (false, "Tòa nhà không tồn tại.", null);
        }

        var existingRooms = await roomRepository.GetAll();
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

        await roomRepository.AddAsync(newRoom);

        var createdRoom = await roomRepository.GetByIdAsync(newRoom.Id);
        return (true, "Tạo phòng thành công.", ToDto(createdRoom!));
    }

    public async Task<(bool Success, string Message)> UpdateRoomAsync(int id, UpdateRoomDto dto)
    {
        var room = await roomRepository.GetByIdAsync(id);
        if (room == null)
            return (false, "Phòng không tồn tại.");

        if (dto.RoomType != null) room.RoomType = dto.RoomType;
        if (dto.Capacity.HasValue) room.Capacity = dto.Capacity.Value;
        if (dto.CurrentOccupancy.HasValue) room.CurrentOccupancy = dto.CurrentOccupancy.Value;
        if (dto.Status != null) room.Status = dto.Status;
        if (dto.Price.HasValue) room.Price = dto.Price.Value;

        await roomRepository.Update(room);
        return (true, "Cập nhật phòng thành công.");
    }

    private static RoomDto ToDto(Room r) => new()
    {
        Id = r.Id,
        BuildingId = r.BuildingId,
        RoomCode = r.RoomCode,
        RoomType = r.RoomType,
        Price = r.Price,
        Capacity = r.Capacity,
        CurrentOccupancy = r.CurrentOccupancy,
        Status = r.Status,
        BuildingCode = r.Building.Code,
        BuildingName = r.Building.Name,
        GenderAllowed = r.Building.GenderAllowed
    };
}
