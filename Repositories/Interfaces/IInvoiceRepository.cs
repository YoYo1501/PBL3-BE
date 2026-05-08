using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
    Task<Room?> GetRoomByCodeAsync(string roomCode);
    Task<List<Contract>> GetActiveContractsByRoomAsync(int roomId);
    Task<bool> InvoiceExistsAsync(int studentId, int roomId, string period);
    Task<bool> PeriodAlreadyImportedAsync(int roomId, string period);
    Task<List<ElectricWaterReading>> GetReadingsByPeriodAsync(string period);
    Task AddReadingAsync(ElectricWaterReading reading);
    Task AddInvoiceAsync(Invoice invoice);
    Task<List<Invoice>> GetDraftInvoicesAsync(string period);
    Task<List<Invoice>> GetMyInvoicesAsync(int studentId);
    Task<List<Invoice>> GetAllInvoicesAsync(string? period, string? status);
    Task<(List<Invoice> Items, int TotalCount)> GetPagedInvoicesAsync(string? period, string? status, int page, int pageSize);
    Task<List<Invoice>> GetPaidInvoicesAsync(string? period, int? studentId);
    Task<(List<Invoice> Items, int TotalCount)> GetPagedPaidInvoicesAsync(string? period, int? studentId, int page, int pageSize);
    Task<Invoice?> GetPaidInvoiceByIdAsync(int invoiceId, int? studentId = null);
    Task UpdateInvoiceAsync(Invoice invoice);
    Task SaveChangesAsync();
}
