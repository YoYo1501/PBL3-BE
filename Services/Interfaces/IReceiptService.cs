using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Receipt.Requests;
using BackendAPI.Models.DTOs.Receipt.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IReceiptService
{
    Task<List<ReceiptResponseDto>> GetMyReceiptsAsync(int studentId, string? period);
    Task<PagedResultDto<ReceiptResponseDto>> GetPagedMyReceiptsAsync(int studentId, ReceiptListQueryDto query);
    Task<ReceiptResponseDto?> GetMyReceiptByInvoiceIdAsync(int studentId, int invoiceId);
    Task<ReceiptExportDto?> ExportMyReceiptAsync(int studentId, int invoiceId);
    Task<List<ReceiptResponseDto>> GetAllReceiptsAsync(string? period, int? studentId);
    Task<PagedResultDto<ReceiptResponseDto>> GetPagedReceiptsAsync(ReceiptListQueryDto query);
    Task<ReceiptResponseDto?> GetReceiptByInvoiceIdAsync(int invoiceId);
    Task<ReceiptExportDto?> ExportReceiptAsync(int invoiceId);
}
