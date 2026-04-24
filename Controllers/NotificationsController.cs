using BackendAPI.Models.DTOs.Notification.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(INotificationService service) : ControllerBase
{
    private int GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(idStr) ? 0 : int.Parse(idStr);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] NotificationFilterDto filter)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize"))
        {
            var paged = await service.GetPagedAsync(filter);
            return Ok(paged);
        }

        var data = await service.GetAllAsync(filter);
        return Ok(data);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationFilterDto filter)
    {
        var userId = GetUserId();
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize"))
        {
            var paged = await service.GetMyPagedNotificationsAsync(userId, filter);
            return Ok(paged);
        }

        var data = await service.GetMyNotificationsAsync(userId);
        return Ok(data);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        var (success, message) = await service.MarkAsReadAsync(id, userId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
