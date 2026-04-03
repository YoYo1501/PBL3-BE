using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
    }
}
        