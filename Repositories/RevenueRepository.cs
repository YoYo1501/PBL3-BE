using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class RevenueRepository(AppDbContext context) : IRevenueRepository
{
    public async Task<List<Invoice>> GetInvoicesAsync(
        DateTime startDate, DateTime endDate,
        string? roomCode, string? period)
    {
        var query = context.Invoices
            .Include(i => i.Student)
            .Include(i => i.Room)
            .Where(i => i.Status != "Draft"
                && i.IssuedAt >= startDate
                && i.IssuedAt <= endDate)
            .AsQueryable();

        if (!string.IsNullOrEmpty(roomCode))
            query = query.Where(i => i.Room.RoomCode == roomCode);

        if (!string.IsNullOrEmpty(period))
            query = query.Where(i => i.Period == period);

        return await query.OrderByDescending(i => i.IssuedAt).ToListAsync();
    }
}