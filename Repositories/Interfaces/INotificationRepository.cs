using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id);
    Task<List<Notification>> GetAllAsync(string? searchText, DateTime? fromDate, DateTime? toDate);
    Task<(List<Notification> Items, int TotalCount)> GetPagedAsync(string? searchText, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<List<Notification>> GetByUserIdAsync(int userId);
    Task<(List<Notification> Items, int TotalCount)> GetPagedByUserIdAsync(string? searchText, DateTime? fromDate, DateTime? toDate, int page, int pageSize, int userId);
    Task AddAsync(Notification notification);
    void Update(Notification notification);
    void Delete(Notification notification);
    Task SaveChangesAsync();
}
