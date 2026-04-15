using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Room>> GetAll()
        => await _context.Rooms
            .Include(r => r.Building)
            .ToListAsync();

    public async Task<Room?> GetByIdAsync(int id) 
        => await _context.Rooms
            .Include(r => r.Building) // cần để check GenderAllowed
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Room?> GetRoomByStudentIdAsync(int studentId)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.Status == "Active" && c.EndDate >= DateTime.UtcNow);
        if (contract == null) return null;
        
        return await _context.Rooms
            .Include(r => r.Building)
            .FirstOrDefaultAsync(r => r.Id == contract.RoomId);
    }

    public async Task AddAsync(Room room)
    {
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Room room)
    {
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> BuildingExistsAsync(int buildingId)
    {
        return await _context.Buildings.AnyAsync(b => b.Id == buildingId);
    }
}