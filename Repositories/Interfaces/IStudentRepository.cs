using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<bool> PhoneExistsAsync(string phone, int excludeStudentId);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Student student);
    Task SaveChangesAsync();
}