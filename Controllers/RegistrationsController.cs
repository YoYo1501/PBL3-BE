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
    [HttpPost("extract-cccd")]
    [AllowAnonymous]
    public async Task<IActionResult> ExtractCccdInfo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng tải lên ảnh CCCD." });
        }

        try
        {
            var extractedData = await ocrService.ExtractCccdInfoAsync(file);
            return Ok(new { success = true, data = extractedData });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi trích xuất dữ liệu: " + ex.Message });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto dto)
    {
        var (success, message, data) = await service.RegisterAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var list = await service.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending([FromQuery] RegistrationListQueryDto query)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize"))
        {
            var paged = await service.GetPagedPendingAsync(query);
            return Ok(paged);
        }

        var list = await service.GetPendingAsync();
        return Ok(list);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveRegistrationRequest dto)
    {
        var (success, message) = await service.ApproveAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
