using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.Facility.Requests;
using BackendAPI.Models.DTOs.Facility.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class FacilityService(IFacilityRepository _repo, IRoomRepository _roomRepo) : IFacilityService
{
    private FacilityResponseDto ToDto(Facility f) => new()
    {
        Id = f.Id,
        RoomId = f.RoomId,
        RoomCode = f.Room?.RoomCode ?? "",
        Name = f.Name,
        Quantity = f.Quantity,
        Status = f.Status,
        CreatedAt = f.CreatedAt
    };

    public async Task<(bool Success, string Message, FacilityResponseDto? Data)> CreateAsync(CreateFacilityDto dto)
    {
        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new BadRequestException("Phòng không t?n t?i");

        var facility = new Facility
        {
            RoomId = dto.RoomId,
            Name = dto.Name,
            Quantity = dto.Quantity,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(facility);
        await _repo.SaveChangesAsync();

        // Gán Room ?? Map ra DTO cho ??y ?? thông tin
        facility.Room = room;

        return (true, "Thêm c? s? v?t ch?t thành công", ToDto(facility));
    }

    public async Task<(bool Success, string Message, FacilityResponseDto? Data)> UpdateAsync(int id, UpdateFacilityDto dto)
    {
        var facility = await _repo.GetByIdAsync(id);
        if (facility == null)
            throw new BadRequestException("Không tìm th?y c? s? v?t ch?t này");

        facility.Name = dto.Name;
        facility.Quantity = dto.Quantity;
        facility.Status = dto.Status;

        _repo.Update(facility);
        await _repo.SaveChangesAsync();

        return (true, "C?p nh?t thành công", ToDto(facility));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var facility = await _repo.GetByIdAsync(id);
        if (facility == null)
            throw new BadRequestException("Không tìm th?y c? s? v?t ch?t này");

        _repo.Delete(facility);
        await _repo.SaveChangesAsync();

        return (true, "Xóa c? s? v?t ch?t thành công");
    }

    public async Task<List<FacilityResponseDto>> GetAllFacilitiesAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<List<FacilityResponseDto>> GetFacilitiesByRoomIdAsync(int roomId)
    {
        var list = await _repo.GetByRoomIdAsync(roomId);
        return list.Select(ToDto).ToList();
    }
}