using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IReceiptRepository
{
    Task AddReceiptAsync(Receipt receipt);
    Task<bool> ReceiptExistsByInvoiceIdAsync(int invoiceId);
    Task<Receipt?> GetReceiptByInvoiceIdAsync(int invoiceId, int? studentId = null);
    Task<List<Receipt>> GetReceiptsAsync(string? period, int? studentId);
    Task<(List<Receipt> Items, int TotalCount)> GetPagedReceiptsAsync(string? period, int? studentId, int page, int pageSize);
    Task SaveChangesAsync();
}
