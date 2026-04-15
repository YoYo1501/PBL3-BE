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
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<StudentRequest>> GetAllAsync(string? status, string? requestType)
    {
        var query = _context.StudentRequests
            .Include(r => r.Student)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);
            
        if (!string.IsNullOrEmpty(requestType))
            query = query.Where(r => r.RequestType == requestType);

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<List<StudentRequest>> GetByStudentIdAsync(int studentId, string? status)
    {
        var query = _context.StudentRequests
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
}