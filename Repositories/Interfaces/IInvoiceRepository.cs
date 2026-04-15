using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
    Task<Room?> GetRoomByCodeAsync(string roomCode);
    Task<List<Contract>> GetActiveContractsByRoomAsync(int roomId);
    Task<bool> PeriodAlreadyImportedAsync(int roomId, string period);
    Task<List<ElectricWaterReading>> GetReadingsByPeriodAsync(string period);
    Task AddReadingAsync(ElectricWaterReading reading);
    Task AddInvoiceAsync(Invoice invoice);
    Task<List<Invoice>> GetDraftInvoicesAsync(string period);
    Task<List<Invoice>> GetMyInvoicesAsync(int studentId);
    Task<List<Invoice>> GetAllInvoicesAsync(string? period, string? status);
    Task UpdateInvoiceAsync(Invoice invoice);
    Task SaveChangesAsync();
}