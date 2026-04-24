using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(int id)
        => await context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

    public async Task<List<Notification>> GetAllAsync(string? searchText, DateTime? fromDate, DateTime? toDate)
        => await BuildQuery(searchText, fromDate, toDate)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<(List<Notification> Items, int TotalCount)> GetPagedAsync(string? searchText, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = BuildQuery(searchText, fromDate, toDate);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId)
        => await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<(List<Notification> Items, int TotalCount)> GetPagedByUserIdAsync(string? searchText, DateTime? fromDate, DateTime? toDate, int page, int pageSize, int userId)
    {
        var query = BuildQuery(searchText, fromDate, toDate)
            .Where(n => n.UserId == userId);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Notification notification)
        => await context.Notifications.AddAsync(notification);

    public void Update(Notification notification)
        => context.Notifications.Update(notification);

    public void Delete(Notification notification)
        => context.Notifications.Remove(notification);

    public async Task SaveChangesAsync()
        => await context.SaveChangesAsync();

    private IQueryable<Notification> BuildQuery(string? searchText, DateTime? fromDate, DateTime? toDate)
    {
        var query = context.Notifications.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearch = searchText.Trim();
            query = query.Where(n => n.Title.Contains(normalizedSearch) || n.Message.Contains(normalizedSearch));
        }

        if (fromDate.HasValue)
            query = query.Where(n => n.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(n => n.CreatedAt <= toDate.Value);

        return query;
    }
}
