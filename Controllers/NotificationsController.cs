using BackendAPI.Models.DTOs.Notification.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class NotificationsController(INotificationService _service) : ControllerBase
{
    private int GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(idStr) ? 0 : int.Parse(idStr);
    }

    [HttpPost]
    
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpPut("{id}")]
    
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpDelete("{id}")]
    
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet]
    
    public async Task<IActionResult> GetAll([FromQuery] NotificationFilterDto filter)
    {
        var data = await _service.GetAllAsync(filter);
        return Ok(data);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetUserId();
        var data = await _service.GetMyNotificationsAsync(userId);
        return Ok(data);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        var (success, message) = await _service.MarkAsReadAsync(id, userId);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
