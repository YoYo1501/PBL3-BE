using BackendAPI.Data;
using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Notification.Requests;
using BackendAPI.Models.DTOs.Notification.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Services;

public class NotificationService(INotificationRepository repo, AppDbContext context) : INotificationService
{
    private static NotificationResponseDto ToDto(Notification notification) => new()
    {
        Id = notification.Id,
        UserId = notification.UserId,
        RecipientName =
            notification.User?.Student?.FullName
            ?? notification.User?.FullName
            ?? notification.User?.Email
            ?? $"User #{notification.UserId}",
        Title = notification.Title,
        Message = notification.Message,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
    };

    private static DateTime ToSecond(DateTime value)
        => new(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);

    private static List<NotificationResponseDto> ToSentHistoryDtos(IEnumerable<Notification> notifications)
        => notifications
            .GroupBy(n => new
            {
                n.Title,
                n.Message,
                CreatedAt = ToSecond(n.CreatedAt)
            })
            .Select(group =>
            {
                var newest = group.OrderByDescending(n => n.CreatedAt).First();
                if (group.Count() == 1) return ToDto(newest);

                return new NotificationResponseDto
                {
                    Id = newest.Id,
                    UserId = 0,
                    RecipientName = "Tất cả sinh viên",
                    IsBroadcast = true,
                    Title = newest.Title,
                    Message = newest.Message,
                    IsRead = group.All(n => n.IsRead),
                    CreatedAt = newest.CreatedAt
                };
            })
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

    private async Task<List<Notification>> GetSentHistoryBatchAsync(Notification notification)
    {
        var sentAt = ToSecond(notification.CreatedAt);
        var sentBefore = sentAt.AddSeconds(1);

        return await context.Notifications
            .Include(n => n.User)
            .ThenInclude(u => u.Student)
            .Where(n =>
                n.User.Role == "Student"
                && n.Title == notification.Title
                && n.Message == notification.Message
                && n.CreatedAt >= sentAt
                && n.CreatedAt < sentBefore)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, NotificationResponseDto? Data)> CreateAsync(CreateNotificationDto dto)
    {
        if (dto.SendToAllStudents)
        {
            var sentAt = DateTime.UtcNow;
            var studentIds = await context.Users
                .Where(u => u.Role == "Student" && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            if (!studentIds.Any())
                throw new BadRequestException("Khong co sinh vien nao dang hoat dong de gui thong bao");

            foreach (var studentId in studentIds)
            {
                await repo.AddAsync(new Notification
                {
                    UserId = studentId,
                    Title = dto.Title,
                    Message = dto.Message,
                    CreatedAt = sentAt,
                    IsRead = false
                });
            }

            await repo.SaveChangesAsync();
            return (true, $"Da gui thong bao cho {studentIds.Count} sinh vien", null);
        }

        if (!dto.UserId.HasValue || dto.UserId.Value <= 0)
            throw new BadRequestException("UserId khong hop le");

        var entity = new Notification
        {
            UserId = dto.UserId.Value,
            Title = dto.Title,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await repo.AddAsync(entity);
        await repo.SaveChangesAsync();

        return (true, "Tao thong bao thanh cong", ToDto(entity));
    }

    public async Task<int> CreateForAdminsAsync(string title, string message)
    {
        var adminIds = await context.Users
            .Where(u => u.Role == "Admin" && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        if (!adminIds.Any()) return 0;

        foreach (var adminId in adminIds)
        {
            await repo.AddAsync(new Notification
            {
                UserId = adminId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }

        await repo.SaveChangesAsync();
        return adminIds.Count;
    }

    public async Task<(bool Success, string Message, NotificationResponseDto? Data)> UpdateAsync(int id, UpdateNotificationDto dto)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing == null) throw new BadRequestException("Thong bao khong ton tai");

        var batch = await GetSentHistoryBatchAsync(existing);
        if (batch.Count <= 1) batch = [existing];

        foreach (var notification in batch)
        {
            notification.Title = dto.Title;
            notification.Message = dto.Message;
            repo.Update(notification);
        }

        await repo.SaveChangesAsync();

        var data = ToSentHistoryDtos(batch).FirstOrDefault() ?? ToDto(existing);
        var message = batch.Count > 1
            ? "Cap nhat thong bao cho tat ca sinh vien thanh cong"
            : "Cap nhat thong bao thanh cong";

        return (true, message, data);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing == null) throw new BadRequestException("Thong bao khong ton tai");

        var batch = await GetSentHistoryBatchAsync(existing);
        if (batch.Count <= 1) batch = [existing];

        foreach (var notification in batch)
        {
            repo.Delete(notification);
        }

        await repo.SaveChangesAsync();

        var message = batch.Count > 1
            ? "Xoa thong bao cho tat ca sinh vien thanh cong"
            : "Xoa thong bao thanh cong";

        return (true, message);
    }

    public async Task<List<NotificationResponseDto>> GetAllAsync(NotificationFilterDto filter)
    {
        var list = await repo.GetAllAsync(filter.SearchText, filter.FromDate, filter.ToDate);
        return ToSentHistoryDtos(list);
    }

    public async Task<PagedResultDto<NotificationResponseDto>> GetPagedAsync(NotificationFilterDto filter)
    {
        var page = filter.GetPage();
        var pageSize = filter.GetPageSize(8);
        var sentHistory = ToSentHistoryDtos(await repo.GetAllAsync(filter.SearchText, filter.FromDate, filter.ToDate));
        var totalCount = sentHistory.Count;
        var items = sentHistory
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<NotificationResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };
    }

    public async Task<List<NotificationResponseDto>> GetMyNotificationsAsync(int userId)
    {
        var list = await repo.GetByUserIdAsync(userId);
        return list.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<NotificationResponseDto>> GetMyPagedNotificationsAsync(int userId, NotificationFilterDto filter)
    {
        var page = filter.GetPage();
        var pageSize = filter.GetPageSize(8);
        var (items, totalCount) = await repo.GetPagedByUserIdAsync(filter.SearchText, filter.FromDate, filter.ToDate, page, pageSize, userId);

        return new PagedResultDto<NotificationResponseDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };
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
