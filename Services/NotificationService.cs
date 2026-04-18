using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Notification.Requests;
using BackendAPI.Models.DTOs.Notification.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class NotificationService(INotificationRepository repo) : INotificationService
{
    private static NotificationResponseDto ToDto(Notification notification) => new()
    {
        Id = notification.Id,
        UserId = notification.UserId,
        Title = notification.Title,
        Message = notification.Message,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
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

        await repo.AddAsync(entity);
        await repo.SaveChangesAsync();

        return (true, "Tao thong bao thanh cong", ToDto(entity));
    }

    public async Task<(bool Success, string Message, NotificationResponseDto? Data)> UpdateAsync(int id, UpdateNotificationDto dto)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing == null) throw new BadRequestException("Thong bao khong ton tai");

        existing.Title = dto.Title;
        existing.Message = dto.Message;

        repo.Update(existing);
        await repo.SaveChangesAsync();

        return (true, "Cap nhat thong bao thanh cong", ToDto(existing));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing == null) throw new BadRequestException("Thong bao khong ton tai");

        repo.Delete(existing);
        await repo.SaveChangesAsync();

        return (true, "Xoa thong bao thanh cong");
    }

    public async Task<List<NotificationResponseDto>> GetAllAsync(NotificationFilterDto filter)
    {
        var list = await repo.GetAllAsync(filter.SearchText, filter.FromDate, filter.ToDate);
        return list.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<NotificationResponseDto>> GetPagedAsync(NotificationFilterDto filter)
    {
        var page = filter.GetPage();
        var pageSize = filter.GetPageSize(8);
        var (items, totalCount) = await repo.GetPagedAsync(filter.SearchText, filter.FromDate, filter.ToDate, page, pageSize);

        return new PagedResultDto<NotificationResponseDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<List<NotificationResponseDto>> GetMyNotificationsAsync(int userId)
    {
        var list = await repo.GetByUserIdAsync(userId);
        return list.Select(ToDto).ToList();
    }

    public async Task<(bool Success, string Message)> MarkAsReadAsync(int id, int userId)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing == null || existing.UserId != userId)
            throw new BadRequestException("Thong bao khong thuoc ve ban hoac khong ton tai");

        existing.IsRead = true;
        repo.Update(existing);
        await repo.SaveChangesAsync();

        return (true, "Da danh dau da doc");
    }
}
