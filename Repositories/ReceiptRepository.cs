using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class ReceiptRepository(AppDbContext context) : IReceiptRepository
{
    public async Task AddReceiptAsync(Receipt receipt)
        => await context.Receipts.AddAsync(receipt);

    public async Task<bool> ReceiptExistsByInvoiceIdAsync(int invoiceId)
        => await context.Receipts.AnyAsync(r => r.InvoiceId == invoiceId);

    public async Task<Receipt?> GetReceiptByInvoiceIdAsync(int invoiceId, int? studentId = null)
    {
        var query = BuildReceiptQuery(null, studentId);
        return await query.FirstOrDefaultAsync(r => r.InvoiceId == invoiceId);
    }

    public async Task<List<Receipt>> GetReceiptsAsync(string? period, int? studentId)
        => await BuildReceiptQuery(period, studentId)
            .OrderByDescending(r => r.PaidAt)
            .ThenByDescending(r => r.Id)
            .ToListAsync();

    public async Task<(List<Receipt> Items, int TotalCount)> GetPagedReceiptsAsync(string? period, int? studentId, int page, int pageSize)
    {
        var query = BuildReceiptQuery(period, studentId);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.PaidAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task SaveChangesAsync()
        => await context.SaveChangesAsync();

    private IQueryable<Receipt> BuildReceiptQuery(string? period, int? studentId)
    {
        var query = context.Receipts
            .Include(r => r.Invoice)
                .ThenInclude(i => i.Student)
            .Include(r => r.Invoice)
                .ThenInclude(i => i.Room)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(period))
            query = query.Where(r => r.Invoice.Period == period);

        if (studentId.HasValue)
            query = query.Where(r => r.Invoice.StudentId == studentId.Value);

        return query;
    }
}
