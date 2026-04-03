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

    public async Task Update(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }
}