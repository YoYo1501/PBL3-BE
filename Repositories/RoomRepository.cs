using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class RoomRepository(AppDbContext context) : IRoomRepository
{
    public async Task<List<Room>> GetAll()
        => await context.Rooms
            .Include(r => r.Building)
            .ToListAsync();

    public async Task SyncOccupancyFromActiveContractsAsync()
    {
        var now = DateTime.UtcNow;
        var activeOccupancy = await context.Contracts
            .Where(c => c.Status == "Active" && c.StartDate <= now && c.EndDate >= now)
            .GroupBy(c => c.RoomId)
            .Select(g => new
            {
                RoomId = g.Key,
                Count = g.Select(c => c.StudentId).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.RoomId, x => x.Count);

        var rooms = await context.Rooms.ToListAsync();
        foreach (var room in rooms)
        {
            var occupancy = activeOccupancy.GetValueOrDefault(room.Id);
            room.CurrentOccupancy = Math.Min(occupancy, room.Capacity);

            if (room.CurrentOccupancy >= room.Capacity)
            {
                room.Status = "Full";
            }
            else if (room.CurrentOccupancy == 0 && room.Status == "Locked")
            {
                room.Status = "Locked";
            }
            else
            {
                room.Status = "Available";
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<(List<Room> Items, int TotalCount)> GetPagedAsync(string? keyword, string? status, int page, int pageSize)
    {
        var query = BuildQuery(keyword, status);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Building.Code)
            .ThenBy(r => r.RoomCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Room?> GetByIdAsync(int id)
        => await context.Rooms
            .Include(r => r.Building)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Room?> GetRoomByStudentIdAsync(int studentId)
    {
        var now = DateTime.UtcNow;
        var contract = await context.Contracts
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.Status == "Active" && c.StartDate <= now && c.EndDate >= now);
        if (contract == null) return null;

        return await context.Rooms
            .Include(r => r.Building)
            .FirstOrDefaultAsync(r => r.Id == contract.RoomId);
    }

    public async Task AddAsync(Room room)
    {
        await context.Rooms.AddAsync(room);
        await context.SaveChangesAsync();
    }

    public async Task Update(Room room)
    {
        context.Rooms.Update(room);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Room room)
    {
        context.Rooms.Remove(room);
        await context.SaveChangesAsync();
    }

    public async Task<bool> BuildingExistsAsync(int buildingId)
        => await context.Buildings.AnyAsync(b => b.Id == buildingId);

    private IQueryable<Room> BuildQuery(string? keyword, string? status)
    {
        var query = context.Rooms
            .Include(r => r.Building)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(r =>
                r.RoomCode.Contains(normalizedKeyword) ||
                r.RoomType.Contains(normalizedKeyword) ||
                r.Building.Name.Contains(normalizedKeyword) ||
                r.Building.Code.Contains(normalizedKeyword));
        }

        return query;
    }
}
