using BackendAPI.Models.DTOs.Invoice.Requests;
using BackendAPI.Models.DTOs.Invoice.Responses;
using Microsoft.AspNetCore.Http;

namespace BackendAPI.Services.Interfaces;

public interface IInvoiceService
{
    Task<(bool Success, string Message, List<ImportResultDto>? Preview)> ImportExcelAsync(IFormFile file, string period);
    Task<(bool Success, string Message, List<InvoiceDraftDto>? Drafts)> GenerateInvoicesAsync(InvoiceSettingDto dto);
    Task<List<InvoiceDraftDto>> GetDraftInvoicesAsync(string period);
    Task<(bool Success, string Message)> PublishInvoicesAsync(string period);
    Task<List<InvoiceDraftDto>> GetMyInvoicesAsync(int studentId);
    Task<byte[]> ExportInvoicesAsync(string period);
    Task<List<InvoiceDraftDto>> GetAllInvoicesAsync(string? period, string? status);
    Task<InvoiceDraftDto?> GetInvoiceByIdAsync(int id);
    Task<(bool Success, string Message)> PayInvoiceManuallyAsync(int id);
    Task<(bool Success, string Message)> RemindDebtAsync(string? period);
}