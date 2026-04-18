using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Contract.Requests;
using BackendAPI.Models.DTOs.Contract.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IContractService
{
    Task<(bool Success, string Message, ContractResponseDto? Data)> GetMyContractAsync(int studentId);
    Task<(bool Success, string Message, List<RenewalPackageResponseDto>? Packages)> GetRenewalPackagesAsync(int studentId);
    Task<(bool Success, string Message)> SubmitRenewalAsync(int studentId, RenewalRequestDto dto);
    Task<List<RenewalResponseDto>> GetAllPendingRenewalsAsync();
    Task<(bool Success, string Message)> ApproveRenewalAsync(int requestId, ApproveRenewalDto dto);
    Task<List<ContractResponseDto>> GetAllContractsAsync();
    Task<PagedResultDto<ContractResponseDto>> GetPagedContractsAsync(ContractListQueryDto query);
    Task<ContractResponseDto?> GetContractByIdAsync(int id);
    Task<(bool Success, string Message)> UpdateContractAsync(int id, UpdateContractRequestDto dto);
    Task<(bool Success, string Message)> DeleteContractAsync(int id);
}
