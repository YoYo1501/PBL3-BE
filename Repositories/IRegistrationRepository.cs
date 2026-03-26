using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories;

public interface IRegistrationRepository
{
    Task<Registration?> GetByIdAsync(int id);
    Task<List<Registration>> GetAllAsync();
    Task<bool> HasPendingRegistrationAsync(string citizenId);
    Task AddAsync(Registration registration);
    Task SaveChangesAsync();
    Task<List<Registration>> GetPendingAsync();   
    Task UpdateAsync(Registration registration);
}