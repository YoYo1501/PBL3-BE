using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Student.Requests;
using BackendAPI.Models.DTOs.Student.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IStudentService
{
    Task<List<StudentResponseDto>> GetAllAsync();
    Task<PagedResultDto<StudentResponseDto>> GetPagedAsync(StudentListQueryDto query);
    Task<(bool Success, string Message, StudentResponseDto? Data)> GetByIdAsync(int id);
    Task<(bool Success, string Message)> UpdateAsync(int id, UpdateStudentDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
