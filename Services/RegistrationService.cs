using BackendAPI.Data;
using BackendAPI.Models.DTOs.Registration;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IRegistrationRepository _registrationRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly AppDbContext _context;

    public RegistrationService(
        IRegistrationRepository registrationRepo,
        IRoomRepository roomRepo,
        AppDbContext context)
    {
        _registrationRepo = registrationRepo;
        _roomRepo = roomRepo;
        _context = context;
    }

    public async Task<(bool Success, string Message, RegistrationResponseDto? Data)> RegisterAsync(RegistrationRequestDto dto)
    {
        // Kiểm tra CCCD đã có đơn chưa
        var hasPending = await _registrationRepo.HasPendingRegistrationAsync(dto.CitizenId);
        if (hasPending)
            return (false, "CCCD này đã có đơn đăng ký trong hệ thống.", null);

        // Kiểm tra email đã tồn tại chưa
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
            return (false, "Email này đã được sử dụng.", null);

        // Kiểm tra phòng tồn tại không
        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null)
            return (false, "Phòng không tồn tại.", null);

        // Kiểm tra phòng còn chỗ không
        if (room.CurrentOccupancy >= room.Capacity)
            return (false, "Phòng đã đầy, vui lòng chọn phòng khác.", null);

        // Kiểm tra giới tính phù hợp không
        if (room.Building.GenderAllowed != dto.Gender)
            return (false, $"Phòng này chỉ dành cho {room.Building.GenderAllowed}.", null);

        // Chỉ tạo Registration giữ hộ thông tin KH (Guest)
        var registration = new Registration
        {
            RegistrationCode = $"REG_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString()[..6].ToUpper()}",
            RoomId = dto.RoomId,
            FullName = dto.FullName,
            CitizenId = dto.CitizenId,
            Gender = dto.Gender,
            Phone = dto.Phone,
            Email = dto.Email,
            PermanentAddress = dto.PermanentAddress,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = "Pending"
        };
        await _registrationRepo.AddAsync(registration);
        await _registrationRepo.SaveChangesAsync();

        return (true, "Đăng ký thành công! Vui lòng chờ admin duyệt đơn.", new RegistrationResponseDto
        {
            Id = registration.Id,
            RegistrationCode = registration.RegistrationCode,
            FullName = dto.FullName,
            RoomCode = room.RoomCode,
            Status = registration.Status,
            StartDate = registration.StartDate,
            EndDate = registration.EndDate,
            SubmittedAt = registration.SubmittedAt
        });
    }

    public async Task<List<RegistrationResponseDto>> GetAllAsync()
    {
        var list = await _registrationRepo.GetAllAsync();
        return list.Select(r => new RegistrationResponseDto
        {
            Id = r.Id,
            RegistrationCode = r.RegistrationCode,
            FullName = r.FullName,
            RoomCode = r.Room.RoomCode,
            Status = r.Status,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            SubmittedAt = r.SubmittedAt
        }).ToList();
    }
    public async Task<List<RegistrationResponseDto>> GetPendingAsync()
    {
        var list = await _registrationRepo.GetPendingAsync();
        return list.Select(r => new RegistrationResponseDto
        {
            Id = r.Id,
            RegistrationCode = r.RegistrationCode,
            FullName = r.FullName,
            RoomCode = r.Room.RoomCode,
            Status = r.Status,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            SubmittedAt = r.SubmittedAt
        }).ToList();
    }

    public async Task<(bool Success, string Message)> ApproveAsync(int id, ApproveRegistrationDto dto)
    {
        // Lấy đơn đăng ký
        var registration = await _registrationRepo.GetByIdAsync(id);
        if (registration == null)
            return (false, "Không tìm thấy đơn đăng ký.");

        if (registration.Status != "Pending")
            return (false, "Đơn này đã được xử lý rồi.");

        if (dto.IsApproved)
        {
            // Cập nhật số người trong phòng
            var room = await _roomRepo.GetByIdAsync(registration.RoomId);
            if (room == null)
                return (false, "Phòng không tồn tại.");

            if (room.CurrentOccupancy >= room.Capacity)
                return (false, "Phòng đã đầy, không thể duyệt.");

            // Duyệt đơn
            registration.Status = "Approved";

            // 1. Tạo User (username = Email, password = CCCD)
            var user = new User
            {
                Email = registration.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registration.CitizenId),
                Role = "Student",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // 2. Tạo Student
            var student = new Student
            {
                UserId = user.Id,
                FullName = registration.FullName,
                CitizenId = registration.CitizenId,
                Gender = registration.Gender,
                Phone = registration.Phone,
                Email = registration.Email,
                PermanentAddress = registration.PermanentAddress
            };
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            // 3. Liên kết Student vào Đơn & Cập nhật phòng
            registration.StudentId = student.Id;
            room.CurrentOccupancy += 1;
            if (room.CurrentOccupancy >= room.Capacity)
                room.Status = "Full";

            await _roomRepo.Update(room);

            // 4. Tạo hợp đồng
            var contract = new Contract
            {
                ContractCode = $"HD_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString()[..6].ToUpper()}",
                StudentId = student.Id,
                RoomId = registration.RoomId,
                StartDate = registration.StartDate,
                EndDate = registration.EndDate,
                Status = "Active",
                Price = room.RoomType == "4 người" ? 500000 : 350000
            };
            await _context.Contracts.AddAsync(contract);

            // TODO: Gửi email thông báo tài khoản cho sinh viên (làm sau)
            // await _emailService.SendAccountInfoAsync(registration.Email, registration.FullName, registration.CitizenId);
        }
        else
        {
            // Từ chối đơn
            if (string.IsNullOrEmpty(dto.RejectionReason))
                return (false, "Vui lòng nhập lý do từ chối.");

            registration.Status = "Rejected";
            registration.RejectionReason = dto.RejectionReason;
        }

        await _registrationRepo.UpdateAsync(registration);
        await _context.SaveChangesAsync();

        var message = dto.IsApproved
            ? "Duyệt đơn thành công, hợp đồng đã được tạo."
            : "Đã từ chối đơn đăng ký.";

        return (true, message);
    }
}