using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class NotificationRepository(AppDbContext _context) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(int id)
        => await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

    public async Task<List<Notification>> GetAllAsync(string? searchText, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Notifications.AsQueryable();

        if (!string.IsNullOrEmpty(searchText))
            query = query.Where(n => n.Title.Contains(searchText) || n.Message.Contains(searchText));

        if (fromDate.HasValue)
            query = query.Where(n => n.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(n => n.CreatedAt <= toDate.Value);

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId)
        => await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Notification notification)
        => await _context.Notifications.AddAsync(notification);

    public void Update(Notification notification)
        => _context.Notifications.Update(notification);

    public void Delete(Notification notification)
        => _context.Notifications.Remove(notification);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}