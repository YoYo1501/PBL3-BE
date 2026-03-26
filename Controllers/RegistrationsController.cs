using BackendAPI.Models.DTOs.Registration;
using BackendAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _service;

    public RegistrationsController(IRegistrationService service)
    {
        _service = service;
    }

    // POST /api/registrations — sinh viên gửi đơn (không cần login)
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto dto)
    {
        var (success, message, data) = await _service.RegisterAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // GET /api/registrations — admin xem danh sách
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(list);
    }
    // GET /api/registrations/pending — admin xem danh sách chờ duyệt
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var list = await _service.GetPendingAsync();
        return Ok(list);
    }

    // PUT /api/registrations/{id}/approve — admin duyệt hoặc từ chối
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveRegistrationDto dto)
    {
        var (success, message) = await _service.ApproveAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}