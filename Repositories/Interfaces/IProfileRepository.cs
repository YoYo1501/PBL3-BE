using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces
{
    public interface IProfileRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<Student?> GetStudentByUserIdAsync(int userId);
        Task<bool> PhoneExistsAsync(string phone, int excludeUserId);
        Task UpdateStudentAsync(Student student);
        Task UpdateUserAsync(User user);
        Task AddRelativeAsync(Relative relative);
    }
}
