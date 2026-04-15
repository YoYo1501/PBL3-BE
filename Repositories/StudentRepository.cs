using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class StudentRepository(AppDbContext context) : IStudentRepository
{
    public async Task<List<Student>> GetAllAsync()
        => await context.Students
            .Include(s => s.User)
            .Include(s => s.Contracts.Where(c => c.Status == "Active"))
                .ThenInclude(c => c.Room)
            .OrderBy(s => s.FullName)
            .ToListAsync();

    public async Task<Student?> GetByIdAsync(int id)
        => await context.Students
            .Include(s => s.User)
            .Include(s => s.Contracts.Where(c => c.Status == "Active"))
                .ThenInclude(c => c.Room)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<bool> PhoneExistsAsync(string phone, int excludeStudentId)
        => await context.Students
            .AnyAsync(s => s.Phone == phone && s.Id != excludeStudentId);

    public async Task UpdateAsync(Student student)
        => context.Students.Update(student);

    public async Task DeleteAsync(Student student)
        => context.Students.Remove(student);

    public async Task SaveChangesAsync()
        => await context.SaveChangesAsync();
}