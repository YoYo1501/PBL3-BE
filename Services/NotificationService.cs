using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.Notification.Requests;
using BackendAPI.Models.DTOs.Notification.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class NotificationService(INotificationRepository _repo) : INotificationService
{
    private NotificationResponseDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        Title = n.Title,
        Message = n.Message,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };

    public async Task<(bool Success, string Message, NotificationResponseDto? Data)> CreateAsync(CreateNotificationDto dto)
    {
        var entity = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return (true, "T?o thông báo thành công", ToDto(entity));
    }

    public async Task<(bool Success, string Message, NotificationResponseDto? Data)> UpdateAsync(int id, UpdateNotificationDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) throw new BadRequestException("Thông báo không t?n t?i");

        existing.Title = dto.Title;
        existing.Message = dto.Message;

        _repo.Update(existing);
        await _repo.SaveChangesAsync();

        return (true, "C?p nh?t thông báo thành công", ToDto(existing));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) throw new BadRequestException("Thông báo không t?n t?i");

        _repo.Delete(existing);
        await _repo.SaveChangesAsync();

        return (true, "Xóa thông báo thành công");
    }

    public async Task<List<NotificationResponseDto>> GetAllAsync(NotificationFilterDto filter)
    {
        var list = await _repo.GetAllAsync(filter.SearchText, filter.FromDate, filter.ToDate);
        return list.Select(ToDto).ToList();
    }

    public async Task<List<NotificationResponseDto>> GetMyNotificationsAsync(int userId)
    {
        var list = await _repo.GetByUserIdAsync(userId);
        return list.Select(ToDto).ToList();
    }

    public async Task<(bool Success, string Message)> MarkAsReadAsync(int id, int userId)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null || existing.UserId != userId)
            throw new BadRequestException("Thông báo không thu?c v? b?n ho?c không t?n t?i");

        existing.IsRead = true;
        _repo.Update(existing);
        await _repo.SaveChangesAsync();

        return (true, "?ã ?ánh d?u ?ã ??c");
    }
}