using BackendAPI.Data;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;

        public ProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<Student?> GetStudentByUserIdAsync(int userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Relatives)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<bool> PhoneExistsAsync(string phone, int excludeUserId)
        {
            return await _context.Students
                .AnyAsync(s => s.Phone == phone && s.UserId != excludeUserId);
        }

        public async Task UpdateStudentAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task AddRelativeAsync(Relative relative)
        {
            await _context.Relatives.AddAsync(relative);
        }
    }
}
