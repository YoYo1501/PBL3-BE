using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IContractRepository
{
    Task<Contract?> GetActiveContractAsync(int studentId);
    Task<List<RenewalPackages>> GetActivePackagesAsync();
    Task<bool> HasUnpaidInvoiceAsync(int studentId);
    Task<int> CountViolationsAsync(int studentId);
    Task<RenewalRequest?> GetPendingRenewalAsync(int studentId);
    Task<List<RenewalRequest>> GetMyRenewalsAsync(int studentId);
    Task AddRenewalRequestAsync(RenewalRequest request);
    Task<RenewalRequest?> GetRenewalByIdAsync(int id);
    Task<List<RenewalRequest>> GetAllPendingRenewalsAsync();
    Task<List<Contract>> GetAllContractsAsync();
    Task<Contract?> GetContractByIdAsync(int id);
    Task UpdateContractAsync(Contract contract);
    Task UpdateRenewalAsync(RenewalRequest request);
    Task SaveChangesAsync();
}