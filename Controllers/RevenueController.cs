using BackendAPI.Models.DTOs.Revenue.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]

public class RevenueController(IRevenueService service) : ControllerBase
{
    [HttpPost("stats")]
    public async Task<IActionResult> GetRevenue([FromBody] RevenueFilterDto filter)
    {
        var (success, message, data) = await service.GetRevenueAsync(filter);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, data });
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportRevenueExcel([FromBody] RevenueFilterDto filter)
    {
        var fileContent = await service.ExportToExcelAsync(filter);
        if (fileContent == null || fileContent.Length == 0)
            return BadRequest(new { message = "Không có d? li?u d? xu?t." });

        var fileName = $"DoanhThu_{filter.StartDate:yyyyMMdd}_{filter.EndDate:yyyyMMdd}.xlsx";
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}

