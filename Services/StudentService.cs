using BackendAPI.Models.DTOs.Student.Requests;
using BackendAPI.Models.DTOs.Student.Responses;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class StudentService(IStudentRepository repo) : IStudentService
{
    public async Task<List<StudentResponseDto>> GetAllAsync()
    {
        var students = await repo.GetAllAsync();
        return students.Select(ToDto).ToList();
    }

    public async Task<(bool Success, string Message, StudentResponseDto? Data)> GetByIdAsync(int id)
    {
        var student = await repo.GetByIdAsync(id);
        if (student == null)
            return (false, "Không tìm thấy sinh viên.", null);

        return (true, "Lấy thông tin thành công.", ToDto(student));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdateStudentDto dto)
    {
        var student = await repo.GetByIdAsync(id);
        if (student == null)
            return (false, "Không tìm thấy sinh viên.");

        var phoneExists = await repo.PhoneExistsAsync(dto.Phone, id);
        if (phoneExists)
            return (false, "Số điện thoại đã tồn tại trong hệ thống.");

        student.FullName = dto.FullName;
        student.Phone = dto.Phone;
        student.PermanentAddress = dto.PermanentAddress;
        student.User.IsActive = dto.IsActive;

        await repo.UpdateAsync(student);
        await repo.SaveChangesAsync();

        return (true, "Cập nhật thông tin sinh viên thành công.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var student = await repo.GetByIdAsync(id);
        if (student == null)
            return (false, "Không tìm thấy sinh viên.");

        var hasActiveContract = student.Contracts.Any();
        if (hasActiveContract)
            return (false, "Sinh viên đang có hợp đồng lưu trú, không thể xóa.");

        await repo.DeleteAsync(student);
        await repo.SaveChangesAsync();

        return (true, "Xóa sinh viên thành công.");
    }

    private static StudentResponseDto ToDto(BackendAPI.Models.Entities.Student s) => new()
    {
        Id = s.Id,
        FullName = s.FullName,
        CitizenId = s.CitizenId,
        Gender = s.Gender,
        Phone = s.Phone,
        Email = s.Email,
        PermanentAddress = s.PermanentAddress,
        RoomCode = s.Contracts.FirstOrDefault()?.Room.RoomCode ?? "Chưa có phòng",
        IsActive = s.User.IsActive,
        CreatedAt = s.CreatedAt
    };
}