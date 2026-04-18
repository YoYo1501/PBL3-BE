using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class InvoiceRepository(AppDbContext context) : IInvoiceRepository
{
    public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
        => await context.Invoices
            .Include(i => i.Student)
            .Include(i => i.Room)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

    public async Task<Room?> GetRoomByCodeAsync(string roomCode)
        => await context.Rooms
            .Include(r => r.Building)
            .FirstOrDefaultAsync(r => r.RoomCode == roomCode);

    public async Task<List<Contract>> GetActiveContractsByRoomAsync(int roomId)
        => await context.Contracts
            .Include(c => c.Student)
            .Where(c => c.RoomId == roomId
                && c.Status == "Active"
                && c.EndDate >= DateTime.UtcNow)
            .ToListAsync();

    public async Task<bool> PeriodAlreadyImportedAsync(int roomId, string period)
        => await context.ElectricWaterReadings
            .AnyAsync(e => e.RoomId == roomId && e.Period == period);

    public async Task<List<ElectricWaterReading>> GetReadingsByPeriodAsync(string period)
        => await context.ElectricWaterReadings
            .Include(e => e.Room)
            .Where(e => e.Period == period)
            .ToListAsync();

    public async Task AddReadingAsync(ElectricWaterReading reading)
        => await context.ElectricWaterReadings.AddAsync(reading);

    public async Task AddInvoiceAsync(Invoice invoice)
        => await context.Invoices.AddAsync(invoice);

    public async Task<List<Invoice>> GetDraftInvoicesAsync(string period)
        => await BuildInvoiceQuery(period, "Draft")
            .ToListAsync();

    public async Task<List<Invoice>> GetMyInvoicesAsync(int studentId)
        => await context.Invoices
            .Include(i => i.Room)
            .Where(i => i.StudentId == studentId && i.Status != "Draft")
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync();

    public async Task<List<Invoice>> GetAllInvoicesAsync(string? period, string? status)
        => await BuildInvoiceQuery(period, status)
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync();

    public async Task<(List<Invoice> Items, int TotalCount)> GetPagedInvoicesAsync(string? period, string? status, int page, int pageSize)
    {
        var query = BuildInvoiceQuery(period, status);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task UpdateInvoiceAsync(Invoice invoice)
    {
        context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await context.SaveChangesAsync();

    private IQueryable<Invoice> BuildInvoiceQuery(string? period, string? status)
    {
        var query = context.Invoices
            .Include(i => i.Student)
            .Include(i => i.Room)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(period))
            query = query.Where(i => i.Period == period);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status);

        return query;
    }
}
