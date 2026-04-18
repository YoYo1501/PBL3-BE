using BackendAPI.Models.DTOs.Student.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StudentsController(IStudentService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] StudentListQueryDto query)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize") ||
            Request.Query.ContainsKey("keyword") || Request.Query.ContainsKey("isActive"))
        {
            var paged = await service.GetPagedAsync(query);
            return Ok(paged);
        }

        var list = await service.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var (success, message, data) = await service.GetByIdAsync(id);
        if (!success) return NotFound(new { message });
        return Ok(new { message, data });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
    {
        var (success, message) = await service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
