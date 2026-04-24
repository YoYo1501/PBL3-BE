using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IViolationRepository
{
    Task<Student?> GetStudentByCitizenIdAsync(string citizenId);
    Task<List<ViolationRecord>> GetViolationsByStudentIdAsync(int studentId);
    Task<int> GetTotalViolationCountAsync(int studentId);
    Task AddViolationAsync(ViolationRecord violation);
    Task UpdateStudentAsync(Student student);
    Task SaveChangesAsync();
}