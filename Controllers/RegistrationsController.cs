using BackendAPI.Models.DTOs.Registration.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RegistrationsController(IRegistrationService service, IOcrService ocrService) : ControllerBase
{
    // POST /api/registrations/extract-cccd
    [HttpPost("extract-cccd")]
    [AllowAnonymous]
    public async Task<IActionResult> ExtractCccdInfo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng t?i lên ?nh CCCD." });

        try
        {
            var extractedData = await ocrService.ExtractCccdInfoAsync(file);
            return Ok(new { success = true, data = extractedData });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "L?i khi trích xu?t d? li?u: " + ex.Message });
        }
    }
    // POST /api/registrations — sinh viên g?i don (không c?n login)
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto dto)
    {
        var (success, message, data) = await service.RegisterAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    // GET /api/registrations — admin xem danh sách
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var list = await service.GetAllAsync();
        return Ok(list);
    }
    // GET /api/registrations/pending — admin xem danh sách ch? duy?t
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending()
    {
        var list = await service.GetPendingAsync();
        return Ok(list);
    }

    // PUT /api/registrations/{id}/approve — admin duy?t ho?c t? ch?i
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveRegistrationRequest dto)
    {
        var (success, message) = await service.ApproveAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
