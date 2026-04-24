using BackendAPI.Models.DTOs.Violation.Requests;
using BackendAPI.Models.DTOs.Violation.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IViolationService
{
    Task<(bool Success, string Message, StudentViolationInfoDto? Data)> GetStudentViolationInfoAsync(string citizenId);
    Task<(bool Success, string Message, AddViolationResponseDto? Data)> AddViolationAsync(CreateViolationDto dto);
}