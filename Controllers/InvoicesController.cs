using BackendAPI.Models.DTOs.Invoice.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController(IInvoiceService service) : ControllerBase
{
    private int GetStudentId()
    {
        var studentIdStr = User.FindFirstValue("StudentId");
        return string.IsNullOrEmpty(studentIdStr) ? 2 : int.Parse(studentIdStr);
    }

    [HttpPost("import")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportExcel(IFormFile file, [FromQuery] string period)
    {
        var (success, message, preview) = await service.ImportExcelAsync(file, period);
        if (!success) return BadRequest(new { message, preview });
        return Ok(new { message, preview });
    }

    [HttpPost("generate")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateInvoices([FromBody] InvoiceSettingDto dto)
    {
        var (success, message, drafts) = await service.GenerateInvoicesAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message, drafts });
    }

    [HttpGet("draft")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDrafts([FromQuery] string period)
    {
        var list = await service.GetDraftInvoicesAsync(period);
        return Ok(list);
    }

    [HttpPost("publish")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Publish([FromQuery] string period)
    {
        var (success, message) = await service.PublishInvoicesAsync(period);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllInvoices([FromQuery] InvoiceListQueryDto query)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize"))
        {
            var paged = await service.GetPagedInvoicesAsync(query);
            return Ok(paged);
        }

        var list = await service.GetAllInvoicesAsync(query.Period, query.Status);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        var invoice = await service.GetInvoiceByIdAsync(id);
        if (invoice == null) return NotFound(new { message = "Hóa đơn không tồn tại." });
        return Ok(invoice);
    }

    [HttpPut("{id}/pay")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> PayInvoiceManually(int id)
    {
        var (success, message) = await service.PayInvoiceManuallyAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("remind-debt")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemindDebt([FromQuery] string? period)
    {
        var (success, message) = await service.RemindDebtAsync(period);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpGet("my")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyInvoices()
    {
        var studentId = GetStudentId();
        var list = await service.GetMyInvoicesAsync(studentId);
        return Ok(list);
    }

    [HttpGet("export")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportInvoices([FromQuery] string period)
    {
        if (string.IsNullOrEmpty(period))
            return BadRequest(new { message = "Vui lòng cung c?p k? hóa don." });

        var fileContent = await service.ExportInvoicesAsync(period);
        if (fileContent == null || fileContent.Length == 0)
            return BadRequest(new { message = "Không có d? li?u hóa don d? xu?t." });

        var fileName = $"HoaDon_{period}.xlsx";
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
