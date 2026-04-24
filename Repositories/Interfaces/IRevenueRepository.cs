using BackendAPI.Models.Entities;

namespace BackendAPI.Repositories.Interfaces;

public interface IRevenueRepository
{
    Task<List<Invoice>> GetInvoicesAsync(DateTime startDate, DateTime endDate, string? roomCode, string? period);
}