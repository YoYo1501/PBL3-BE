using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Registration.Requests;
using BackendAPI.Models.DTOs.Registration.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IRegistrationService
{
    Task<(bool Success, string Message, RegistrationResponse? Data)> RegisterAsync(RegistrationRequestDto dto);
    Task<List<RegistrationResponse>> GetAllAsync();
    Task<List<RegistrationResponse>> GetPendingAsync();
    Task<PagedResultDto<RegistrationResponse>> GetPagedPendingAsync(RegistrationListQueryDto query);
    Task<(bool Success, string Message)> ApproveAsync(int id, ApproveRegistrationRequest dto);
}
