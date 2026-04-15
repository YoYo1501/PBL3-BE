using BackendAPI.Models.DTOs.Revenue.Requests;
using BackendAPI.Models.DTOs.Revenue.Responses;

namespace BackendAPI.Services.Interfaces;

public interface IRevenueService
{
    Task<(bool Success, string Message, RevenueResponseDto? Data)> GetRevenueAsync(RevenueFilterDto filter);
    Task<byte[]> ExportToExcelAsync(RevenueFilterDto filter);
}