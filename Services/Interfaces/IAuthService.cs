using BackendAPI.Models.DTOs.Auth.Requests;
using BackendAPI.Models.DTOs.Auth.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IAuthService
{
    Task<(LoginResponse? Data, string? Error)> LoginAsync(LoginRequest dto);
}