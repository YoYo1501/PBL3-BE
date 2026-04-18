using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Notification.Requests;
using BackendAPI.Models.DTOs.Notification.Responses;

namespace BackendAPI.Services.Interfaces;

public interface INotificationService
{
    Task<(bool Success, string Message, NotificationResponseDto? Data)> CreateAsync(CreateNotificationDto dto);
    Task<(bool Success, string Message, NotificationResponseDto? Data)> UpdateAsync(int id, UpdateNotificationDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<List<NotificationResponseDto>> GetAllAsync(NotificationFilterDto filter);
    Task<PagedResultDto<NotificationResponseDto>> GetPagedAsync(NotificationFilterDto filter);
    Task<List<NotificationResponseDto>> GetMyNotificationsAsync(int userId);
    Task<(bool Success, string Message)> MarkAsReadAsync(int id, int userId);
}
