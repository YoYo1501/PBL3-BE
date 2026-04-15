using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class FacilityRepository(AppDbContext _context) : IFacilityRepository
{
    public async Task<Facility?> GetByIdAsync(int id)
        => await _context.Facilities
            .Include(f => f.Room)
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<List<Facility>> GetAllAsync()
        => await _context.Facilities
            .Include(f => f.Room)
            .OrderBy(f => f.RoomId)
            .ThenBy(f => f.Name)
            .ToListAsync();

    public async Task<List<Facility>> GetByRoomIdAsync(int roomId)
        => await _context.Facilities
            .Include(f => f.Room)
            .Where(f => f.RoomId == roomId)
            .OrderBy(f => f.Name)
            .ToListAsync();

    public async Task AddAsync(Facility facility)
        => await _context.Facilities.AddAsync(facility);

    public void Update(Facility facility)
        => _context.Facilities.Update(facility);

    public void Delete(Facility facility)
    {
        facility.IsDeleted = true;
        _context.Facilities.Update(facility);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}