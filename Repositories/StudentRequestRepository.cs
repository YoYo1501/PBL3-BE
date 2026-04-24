using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class StudentRequestRepository(AppDbContext _context) : IStudentRequestRepository
{
    public async Task<StudentRequest?> GetByIdAsync(int id)
        => await _context.StudentRequests
            .Include(r => r.Student)
                .ThenInclude(s => s.Contracts.Where(c => c.Status == "Active"))
                    .ThenInclude(c => c.Room)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<StudentRequest>> GetAllAsync(string? status, string? requestType)
    {
        return await BuildQuery(status, requestType)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<StudentRequest> Items, int TotalCount)> GetPagedAsync(string? status, string? requestType, int page, int pageSize)
    {
        var query = BuildQuery(status, requestType);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<StudentRequest>> GetByStudentIdAsync(int studentId, string? status)
    {
        var query = _context.StudentRequests
            .Include(r => r.Student)
                .ThenInclude(s => s.Contracts.Where(c => c.Status == "Active"))
                    .ThenInclude(c => c.Room)
            .Where(r => r.StudentId == studentId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task AddAsync(StudentRequest request)
        => await _context.StudentRequests.AddAsync(request);

    public void Update(StudentRequest request)
        => _context.StudentRequests.Update(request);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    private IQueryable<StudentRequest> BuildQuery(string? status, string? requestType)
    {
        var query = _context.StudentRequests
            .Include(r => r.Student)
                .ThenInclude(s => s.Contracts.Where(c => c.Status == "Active"))
                    .ThenInclude(c => c.Room)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        if (!string.IsNullOrEmpty(requestType))
            query = query.Where(r => r.RequestType == requestType);

        return query;
    }
}
