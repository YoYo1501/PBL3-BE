using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class StudentRepository(AppDbContext context) : IStudentRepository
{
    public async Task<List<Student>> GetAllAsync()
        => await BuildQuery(null, null)
            .OrderBy(s => s.FullName)
            .ToListAsync();

    public async Task<(List<Student> Items, int TotalCount)> GetPagedAsync(string? keyword, bool? isActive, int page, int pageSize)
    {
        var query = BuildQuery(keyword, isActive);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Student?> GetByIdAsync(int id)
        => await context.Students
            .Include(s => s.User)
            .Include(s => s.Contracts.Where(c => c.Status == "Active"))
                .ThenInclude(c => c.Room)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<bool> PhoneExistsAsync(string phone, int excludeStudentId)
        => await context.Students
            .AnyAsync(s => s.Phone == phone && s.Id != excludeStudentId);

    public Task UpdateAsync(Student student)
    {
        context.Students.Update(student);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Student student)
    {
        context.Students.Remove(student);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await context.SaveChangesAsync();

    private IQueryable<Student> BuildQuery(string? keyword, bool? isActive)
    {
        var query = context.Students
            .Include(s => s.User)
            .Include(s => s.Contracts.Where(c => c.Status == "Active"))
                .ThenInclude(c => c.Room)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(s => s.User.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(s =>
                s.FullName.Contains(normalizedKeyword) ||
                s.CitizenId.Contains(normalizedKeyword) ||
                (s.Phone != null && s.Phone.Contains(normalizedKeyword)) ||
                s.Contracts.Any(c => c.Status == "Active" && c.Room != null && c.Room.RoomCode.Contains(normalizedKeyword)));
        }

        return query;
    }
}
