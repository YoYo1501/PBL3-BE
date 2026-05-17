using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.Facility.Requests;
using BackendAPI.Models.DTOs.Facility.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace BackendAPI.Services;

public class FacilityService(IFacilityRepository _repo, IRoomRepository _roomRepo, IStudentRequestRepository _studentRequestRepo) : IFacilityService
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

        await SyncMaintenanceRequestStatusAsync(facility);

        _repo.Update(facility);
        await _repo.SaveChangesAsync();

        return (true, "C?p nh?t thành công", ToDto(facility));
    }

    private async Task SyncMaintenanceRequestStatusAsync(Facility facility)
    {
        var nextRequestStatus = facility.Status switch
        {
            "Damaged" => "Approved",
            "UnderMaintenance" => "InProgress",
            "Good" => "Completed",
            _ => null
        };

        if (nextRequestStatus == null)
            return;

        var request = await FindMatchingMaintenanceRequestAsync(facility, nextRequestStatus);
        if (request == null || request.Status == nextRequestStatus)
            return;

        request.Status = nextRequestStatus;
        if (nextRequestStatus is "Approved" or "Completed")
            request.ResolvedAt = DateTime.UtcNow;

        _studentRequestRepo.Update(request);
    }

    private async Task<StudentRequest?> FindMatchingMaintenanceRequestAsync(Facility facility, string nextRequestStatus)
    {
        var normalizedFacilityName = NormalizeFacilityName(facility.Name);
        var requests = await _studentRequestRepo.GetMaintenanceRequestsByRoomIdAsync(facility.RoomId);
        var matches = requests
            .Where(request => NormalizeFacilityName(ExtractFacilityName(request.Title)) == normalizedFacilityName)
            .ToList();

        return nextRequestStatus switch
        {
            "Approved" => matches.FirstOrDefault(request => request.Status is "Pending" or "Approved"),
            "InProgress" => matches.FirstOrDefault(request => request.Status is "Pending" or "Approved" or "InProgress"),
            "Completed" => matches.FirstOrDefault(request => request.Status is "Pending" or "Approved" or "InProgress"),
            _ => null
        };
    }

    private static string ExtractFacilityName(string title)
    {
        var safeTitle = title ?? string.Empty;
        var colonIndex = safeTitle.IndexOf(':');
        return (colonIndex >= 0 ? safeTitle[..colonIndex] : safeTitle).Trim();
    }

    private static string NormalizeFacilityName(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            if (!char.IsWhiteSpace(ch))
                builder.Append(ch == '\u0111' ? 'd' : ch);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
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