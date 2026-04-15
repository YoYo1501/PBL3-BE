using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IStudentRequestRepository
{
    Task<StudentRequest?> GetByIdAsync(int id);
    Task<List<StudentRequest>> GetAllAsync(string? status, string? requestType);
    Task<List<StudentRequest>> GetByStudentIdAsync(int studentId, string? status);
    Task AddAsync(StudentRequest request);
    void Update(StudentRequest request);
    Task SaveChangesAsync();
}