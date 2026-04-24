using BackendAPI.Models.DTOs.StudentRequest.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentRequestsController(IStudentRequestService _service) : ControllerBase
{
    private int GetStudentId()
    {
        var idStr = User.FindFirstValue("StudentId");
        return string.IsNullOrEmpty(idStr) ? 0 : int.Parse(idStr);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateStudentRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.CreateRequestAsync(GetStudentId(), dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests([FromQuery] string? status)
    {
        var data = await _service.GetMyRequestsAsync(GetStudentId(), status);
        return Ok(data);
    }
    
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelRequest(int id)
    {
        var (success, message) = await _service.CancelRequestAsync(GetStudentId(), id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllRequests([FromQuery] StudentRequestListQueryDto query)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize"))
        {
            var paged = await _service.GetPagedRequestsAsync(query);
            return Ok(paged);
        }

        var data = await _service.GetAllRequestsAsync(query.Status, query.RequestType);
        return Ok(data);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRequestStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _service.UpdateRequestStatusAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
