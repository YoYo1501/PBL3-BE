using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class ContractRepository(AppDbContext context) : IContractRepository
{
    public async Task<Contract?> GetActiveContractAsync(int studentId)
        => await context.Contracts
            .Include(c => c.Room)
                .ThenInclude(r => r.Building)
            .FirstOrDefaultAsync(c => c.StudentId == studentId
                && c.Status == "Active"
                && c.EndDate >= DateTime.UtcNow);

    public async Task<List<RenewalPackages>> GetActivePackagesAsync()
        => await context.RenewalPackages
            .Where(p => p.IsActive)
            .ToListAsync();

    public async Task<bool> HasUnpaidInvoiceAsync(int studentId)
        => await context.Invoices
            .AnyAsync(i => i.StudentId == studentId && i.Status == "Unpaid");

    public async Task<int> CountViolationsAsync(int studentId)
        => await context.ViolationRecords
            .Where(v => v.StudentId == studentId)
            .SumAsync(v => v.TotalCount);

    public async Task<RenewalRequest?> GetPendingRenewalAsync(int studentId)
        => await context.RenewalRequests
            .FirstOrDefaultAsync(r => r.StudentId == studentId && r.Status == "Pending");

    public async Task AddRenewalRequestAsync(RenewalRequest request)
        => await context.RenewalRequests.AddAsync(request);

    public async Task<RenewalRequest?> GetRenewalByIdAsync(int id)
        => await context.RenewalRequests
            .Include(r => r.Student)
            .Include(r => r.Contract)
            .Include(r => r.RenewalPackage)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<RenewalRequest>> GetAllPendingRenewalsAsync()
        => await context.RenewalRequests
            .Include(r => r.Student)
            .Include(r => r.Contract)
                .ThenInclude(c => c.Room)
            .Include(r => r.RenewalPackage)
            .Where(r => r.Status == "Pending")
            .ToListAsync();

    public async Task<List<Contract>> GetAllContractsAsync()
        => await BuildContractQuery(null, null)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();

    public async Task<(List<Contract> Items, int TotalCount)> GetPagedContractsAsync(string? keyword, string? status, int page, int pageSize)
    {
        var query = BuildContractQuery(keyword, status);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Contract?> GetContractByIdAsync(int id)
        => await context.Contracts
            .Include(c => c.Room)
                .ThenInclude(r => r.Building)
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

    public Task UpdateContractAsync(Contract contract)
    {
        context.Contracts.Update(contract);
        return Task.CompletedTask;
    }

    public Task UpdateRenewalAsync(RenewalRequest request)
    {
        context.RenewalRequests.Update(request);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await context.SaveChangesAsync();

    private IQueryable<Contract> BuildContractQuery(string? keyword, string? status)
    {
        var query = context.Contracts
            .Include(c => c.Room)
                .ThenInclude(r => r.Building)
            .Include(c => c.Student)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(c =>
                c.ContractCode.Contains(normalizedKeyword) ||
                (c.Student != null && c.Student.FullName.Contains(normalizedKeyword)) ||
                (c.Room != null && c.Room.RoomCode.Contains(normalizedKeyword)) ||
                (c.Room != null && c.Room.RoomType.Contains(normalizedKeyword)));
        }

        return query;
    }
}
