using BackendAPI.Models.DTOs.Facility.Requests;
using BackendAPI.Models.DTOs.Facility.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IFacilityService
{
    Task<(bool Success, string Message, FacilityResponseDto? Data)> CreateAsync(CreateFacilityDto dto);
    Task<(bool Success, string Message, FacilityResponseDto? Data)> UpdateAsync(int id, UpdateFacilityDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<List<FacilityResponseDto>> GetAllFacilitiesAsync();
    Task<List<FacilityResponseDto>> GetFacilitiesByRoomIdAsync(int roomId);
}