using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class RoomTransferRepository : IRoomTransferRepository
{
    private readonly AppDbContext _context;

    public RoomTransferRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Contract?> GetActiveContractAsync(int studentId)
        => await _context.Contracts
            .FirstOrDefaultAsync(c => c.StudentId == studentId
                && c.Status == "Active"
                && c.EndDate >= DateTime.UtcNow);

    public async Task<int> CountTransferInSemesterAsync(int studentId, int semesterId)
        => await _context.RoomTransferRequests
            .CountAsync(r => r.StudentId == studentId
                && r.SemesterId == semesterId
                && r.Status != "Rejected");

    public async Task<SemesterPeriods?> GetCurrentSemesterAsync()
        => await _context.SemesterPeriods
            .FirstOrDefaultAsync(s => s.StartDate <= DateTime.UtcNow
                && s.EndDate >= DateTime.UtcNow);

    public async Task<List<Room>> GetAvailableRoomsAsync(string gender, int excludeRoomId)
        => await _context.Rooms
            .Include(r => r.Building)
            .Where(r => r.Building.GenderAllowed == gender
                && r.CurrentOccupancy < r.Capacity
                && r.Status != "Locked"
                && r.Id != excludeRoomId)
            .ToListAsync();

    public async Task<Room?> GetRoomByIdAsync(int roomId)
        => await _context.Rooms
            .Include(r => r.Building)
            .FirstOrDefaultAsync(r => r.Id == roomId);

    public async Task<RoomTransferRequest?> GetPendingTransferAsync(int studentId)
        => await _context.RoomTransferRequests
            .FirstOrDefaultAsync(r => r.StudentId == studentId
                && r.Status == "Pending");

    public async Task AddAsync(RoomTransferRequest request)
        => await _context.RoomTransferRequests.AddAsync(request);

    public async Task<List<RoomTransferRequest>> GetAllPendingAsync()
        => await _context.RoomTransferRequests
            .Include(r => r.Student)
            .Include(r => r.FromRoom)
            .Include(r => r.ToRoom)
            .Where(r => r.Status == "Pending")
            .ToListAsync();

    public async Task UpdateRoomAsync(Room room)
        => _context.Rooms.Update(room);

    public async Task UpdateTransferAsync(RoomTransferRequest request)
        => _context.RoomTransferRequests.Update(request);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}