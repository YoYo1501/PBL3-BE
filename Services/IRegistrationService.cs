using BackendAPI.Models.DTOs.Registration;

namespace BackendAPI.Services;

public interface IRegistrationService
{
    Task<(bool Success, string Message, RegistrationResponseDto? Data)> RegisterAsync(RegistrationRequestDto dto);
    Task<List<RegistrationResponseDto>> GetAllAsync();
    Task<List<RegistrationResponseDto>> GetPendingAsync();                                    
    Task<(bool Success, string Message)> ApproveAsync(int id, ApproveRegistrationDto dto);   
}