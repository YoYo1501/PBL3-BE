using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class ContractRepository(AppDbContext _context) : IContractRepository
{
    public async Task<Contract?> GetActiveContractAsync(int studentId)
        => await _context.Contracts
            .Include(c => c.Room)
                .ThenInclude(r => r.Building)
            .FirstOrDefaultAsync(c => c.StudentId == studentId
                && c.Status == "Active"
                && c.EndDate >= DateTime.UtcNow);

    public async Task<List<RenewalPackages>> GetActivePackagesAsync()
        => await _context.RenewalPackages
            .Where(p => p.IsActive)
            .ToListAsync();

    public async Task<bool> HasUnpaidInvoiceAsync(int studentId)
        => await _context.Invoices
            .AnyAsync(i => i.StudentId == studentId && i.Status == "Unpaid");

    public async Task<int> CountViolationsAsync(int studentId)
        => await _context.ViolationRecords
            .Where(v => v.StudentId == studentId)
            .SumAsync(v => v.TotalCount);

    public async Task<RenewalRequest?> GetPendingRenewalAsync(int studentId)
        => await _context.RenewalRequests
            .FirstOrDefaultAsync(r => r.StudentId == studentId && r.Status == "Pending");

    public async Task AddRenewalRequestAsync(RenewalRequest request)
        => await _context.RenewalRequests.AddAsync(request);

    public async Task<RenewalRequest?> GetRenewalByIdAsync(int id)
        => await _context.RenewalRequests
            .Include(r => r.Student)
            .Include(r => r.Contract)
            .Include(r => r.RenewalPackage)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<RenewalRequest>> GetAllPendingRenewalsAsync()
        => await _context.RenewalRequests
            .Include(r => r.Student)
            .Include(r => r.Contract)
                .ThenInclude(c => c.Room)
            .Include(r => r.RenewalPackage)
            .Where(r => r.Status == "Pending")
            .ToListAsync();

    public async Task<List<Contract>> GetAllContractsAsync()
        => await _context.Contracts
            .Include(c => c.Room)
                .ThenInclude(r => r.Building)
            .Include(c => c.Student)
            .ToListAsync();

    public async Task<Contract?> GetContractByIdAsync(int id)
        => await _context.Contracts
            .Include(c => c.Room)
                .ThenInclude(r => r.Building)
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task UpdateContractAsync(Contract contract)
        => _context.Contracts.Update(contract);

    public async Task UpdateRenewalAsync(RenewalRequest request)
        => _context.RenewalRequests.Update(request);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}