using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IStudentRequestRepository
{
    Task<StudentRequest?> GetByIdAsync(int id);
    Task<List<StudentRequest>> GetAllAsync(string? status, string? requestType);
    Task<(List<StudentRequest> Items, int TotalCount)> GetPagedAsync(string? status, string? requestType, int page, int pageSize);
    Task<List<StudentRequest>> GetByStudentIdAsync(int studentId, string? status);
    Task<List<StudentRequest>> GetMaintenanceRequestsByRoomIdAsync(int roomId);
    Task<bool> HasPendingRequestAsync(int studentId, string requestType);
    Task AddAsync(StudentRequest request);
    void Update(StudentRequest request);
    Task SaveChangesAsync();
}
