using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IRegistrationRepository
{
    Task<Registration?> GetByIdAsync(int id);
    Task<List<Registration>> GetAllAsync();
    Task<(List<Registration> Items, int TotalCount)> GetPagedPendingAsync(int page, int pageSize);
    Task<bool> HasPendingRegistrationAsync(string citizenId);
    Task AddAsync(Registration registration);
    Task SaveChangesAsync();
    Task<List<Registration>> GetPendingAsync();   
    Task UpdateAsync(Registration registration);
}
