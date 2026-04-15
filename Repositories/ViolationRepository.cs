using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories;

public class ViolationRepository(AppDbContext context) : IViolationRepository
{
    public async Task<Student?> GetStudentByCitizenIdAsync(string citizenId)
        => await context.Students
            .Include(s => s.User)
            .Include(s => s.Contracts.Where(c => c.Status == "Active"))
                .ThenInclude(c => c.Room)
            .FirstOrDefaultAsync(s => s.CitizenId == citizenId);

    public async Task<List<ViolationRecord>> GetViolationsByStudentIdAsync(int studentId)
        => await context.ViolationRecords
            .Where(v => v.StudentId == studentId)
            .OrderByDescending(v => v.ViolationDate)
            .ToListAsync();

    public async Task<int> GetTotalViolationCountAsync(int studentId)
        => await context.ViolationRecords
            .Where(v => v.StudentId == studentId)
            .SumAsync(v => v.TotalCount);

    public async Task AddViolationAsync(ViolationRecord violation)
        => await context.ViolationRecords.AddAsync(violation);

    public async Task UpdateStudentAsync(Student student)
        => context.Students.Update(student);

    public async Task SaveChangesAsync()
        => await context.SaveChangesAsync();
}