using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.StudentRequest.Requests;
using BackendAPI.Models.DTOs.StudentRequest.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IStudentRequestService
{
    Task<(bool Success, string Message, StudentRequestResponseDto? Data)> CreateRequestAsync(int studentId, CreateStudentRequestDto dto);
    Task<(bool Success, string Message)> CancelRequestAsync(int studentId, int requestId);
    Task<(bool Success, string Message)> UpdateRequestStatusAsync(int id, UpdateRequestStatusDto dto);
    Task<List<StudentRequestResponseDto>> GetAllRequestsAsync(string? status, string? requestType);
    Task<PagedResultDto<StudentRequestResponseDto>> GetPagedRequestsAsync(StudentRequestListQueryDto query);
    Task<List<StudentRequestResponseDto>> GetMyRequestsAsync(int studentId, string? status);
}
