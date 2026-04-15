using BackendAPI.Models.DTOs.Violation.Requests;
using BackendAPI.Models.DTOs.Violation.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class ViolationService(IViolationRepository repo) : IViolationService
{
    public async Task<(bool Success, string Message, StudentViolationInfoDto? Data)> GetStudentViolationInfoAsync(string citizenId)
    {
        var student = await repo.GetStudentByCitizenIdAsync(citizenId);
        if (student == null)
            return (false, "Không tìm thấy sinh viên với CCCD này.", null);

        var violations = await repo.GetViolationsByStudentIdAsync(student.Id);
        var activeContract = student.Contracts.FirstOrDefault();

        var data = new StudentViolationInfoDto
        {
            FullName = student.FullName,
            CitizenId = student.CitizenId,
            RoomCode = activeContract?.Room.RoomCode ?? "Chưa có phòng",
            TotalViolations = violations.Sum(v => v.TotalCount),
            History = violations.Select(v => new ViolationResponseDto
            {
                Id = v.Id,
                ViolationType = v.ViolationType,
                Description = v.Description,
                ViolationDate = v.ViolationDate,
                Evidence = v.Evidence,
                TotalCount = v.TotalCount
            }).ToList()
        };

        return (true, "Lấy thông tin thành công.", data);
    }

    public async Task<(bool Success, string Message, AddViolationResponseDto? Data)> AddViolationAsync(CreateViolationDto dto)
    {
        var student = await repo.GetStudentByCitizenIdAsync(dto.CitizenId);
        if (student == null)
            return (false, "Không tìm thấy sinh viên.", null);

        // Tạo bản ghi vi phạm
        var violation = new ViolationRecord
        {
            StudentId = student.Id,
            ViolationType = dto.ViolationType,
            Description = dto.Description,
            ViolationDate = dto.ViolationDate,
            Evidence = dto.Evidence,
            TotalCount = 1
        };

        await repo.AddViolationAsync(violation);
        await repo.SaveChangesAsync();

        // Tính tổng vi phạm
        var totalViolations = await repo.GetTotalViolationCountAsync(student.Id);

        string handleResult;

        if (totalViolations >= 5)
        {
            // Khóa tài khoản
            student.User.IsActive = false;
            await repo.UpdateStudentAsync(student);
            await repo.SaveChangesAsync();

            handleResult = $"Sinh viên đã vi phạm {totalViolations} lần. " +
                           "Tài khoản bị khóa, chờ xử lý kỷ luật xóa tên khỏi KTX.";
        }
        else
        {
            handleResult = $"Sinh viên đã vi phạm {totalViolations} lần. " +
                           "Gửi thông báo nhắc nhở đến sinh viên.";
        }

        return (true, "Ghi nhận vi phạm thành công.", new AddViolationResponseDto
        {
            StudentName = student.FullName,
            TotalViolations = totalViolations,
            HandleResult = handleResult
        });
    }
}