using BackendAPI.Models.DTOs.Receipt.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController(IReceiptService receiptService) : ControllerBase
{
    private int GetStudentId()
    {
        var studentIdStr = User.FindFirstValue("StudentId");
        return string.IsNullOrEmpty(studentIdStr) ? 2 : int.Parse(studentIdStr);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyReceipts([FromQuery] ReceiptListQueryDto query)
    {
        var studentId = GetStudentId();
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize"))
        {
            var paged = await receiptService.GetPagedMyReceiptsAsync(studentId, query);
            return Ok(paged);
        }

        var list = await receiptService.GetMyReceiptsAsync(studentId, query.Period);
        return Ok(list);
    }

    [HttpGet("my/{invoiceId:int}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyReceiptByInvoiceId(int invoiceId)
    {
        var studentId = GetStudentId();
        var receipt = await receiptService.GetMyReceiptByInvoiceIdAsync(studentId, invoiceId);
        if (receipt == null)
            return NotFound(new { message = "Bien lai khong ton tai hoac hoa don chua thanh toan." });

        return Ok(receipt);
    }

    [HttpGet("my/{invoiceId:int}/download")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> DownloadMyReceipt(int invoiceId)
    {
        var studentId = GetStudentId();
        var file = await receiptService.ExportMyReceiptAsync(studentId, invoiceId);
        if (file == null)
            return NotFound(new { message = "Bien lai khong ton tai hoac hoa don chua thanh toan." });

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllReceipts([FromQuery] ReceiptListQueryDto query)
    {
        if (Request.Query.ContainsKey("page") || Request.Query.ContainsKey("pageSize"))
        {
            var paged = await receiptService.GetPagedReceiptsAsync(query);
            return Ok(paged);
        }

        var list = await receiptService.GetAllReceiptsAsync(query.Period, query.StudentId);
        return Ok(list);
    }

    [HttpGet("{invoiceId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReceiptByInvoiceId(int invoiceId)
    {
        var receipt = await receiptService.GetReceiptByInvoiceIdAsync(invoiceId);
        if (receipt == null)
            return NotFound(new { message = "Bien lai khong ton tai hoac hoa don chua thanh toan." });

        return Ok(receipt);
    }

    [HttpGet("{invoiceId:int}/download")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DownloadReceipt(int invoiceId)
    {
        var file = await receiptService.ExportReceiptAsync(invoiceId);
        if (file == null)
            return NotFound(new { message = "Bien lai khong ton tai hoac hoa don chua thanh toan." });

        return File(file.Content, file.ContentType, file.FileName);
    }
}
