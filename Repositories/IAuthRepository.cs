using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
    }
}
        