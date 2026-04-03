using BackendAPI.Models.DTOs.Profile.Requests;
using BackendAPI.Models.DTOs.Profile.Responses;

namespace BackendAPI.Services.Interfaces
{
    public interface IProfileService
    {
        Task<(bool Success, string Message, UserProfileResponse? Data)> GetProfileAsync(int userId);
        Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    }
}
