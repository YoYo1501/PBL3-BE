using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class RegistrationRepository : IRegistrationRepository
{
    private readonly AppDbContext _context;

    public RegistrationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Registration?> GetByIdAsync(int id)
        => await _context.Registrations
            .Include(r => r.Student)
            .Include(r => r.Room)
                .ThenInclude(r => r.Building)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<Registration>> GetAllAsync()
        => await _context.Registrations
            .Include(r => r.Student)
            .Include(r => r.Room)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

    public async Task<(List<Registration> Items, int TotalCount)> GetPagedPendingAsync(int page, int pageSize)
    {
        var query = _context.Registrations
            .Include(r => r.Student)
            .Include(r => r.Room)
            .Where(r => r.Status == "Pending");

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> HasPendingRegistrationAsync(string citizenId)
        => await _context.Registrations
            .AnyAsync(r => r.CitizenId == citizenId
                && (r.Status == "Pending" || r.Status == "Approved"));

    public async Task AddAsync(Registration registration)
        => await _context.Registrations.AddAsync(registration);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
    public async Task<List<Registration>> GetPendingAsync()
    => await _context.Registrations
        .Include(r => r.Student)
        .Include(r => r.Room)
        .Where(r => r.Status == "Pending")
        .OrderByDescending(r => r.SubmittedAt)
        .ToListAsync();

    public async Task UpdateAsync(Registration registration)
    {
        _context.Registrations.Update(registration);
        await _context.SaveChangesAsync();
    }
}
