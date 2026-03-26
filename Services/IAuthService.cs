using BackendAPI.Models.DTOs.Auth;

namespace BackendAPI.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}