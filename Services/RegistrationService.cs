using BackendAPI.Data;
using BackendAPI.Models.DTOs.Registration.Requests;
using BackendAPI.Models.DTOs.Registration.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BackendAPI.Exceptions;

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

    public async Task<(bool Success, string Message, RegistrationResponse? Data)> RegisterAsync(RegistrationRequestDto dto)
    {
        dto.Email = dto.Email.Trim();
        dto.FullName = dto.FullName.Trim();
        dto.Phone = dto.Phone.Trim();
        dto.CitizenId = dto.CitizenId.Trim();
        dto.Email = dto.Email.ToLower();
        // Kiểm tra CCCD đã có đơn chưa
        var hasPending = await _registrationRepo.HasPendingRegistrationAsync(dto.CitizenId);
        if (hasPending)
            throw new BadRequestException("CCCD này đã có đơn đăng ký.");
            
        var cccdExistsInStudent = await _context.Students.AnyAsync(s => s.CitizenId == dto.CitizenId);
        if (cccdExistsInStudent)
            throw new BadRequestException("CCCD này đã được sử dụng trong hệ thống.");

        var hasActiveContract = await _context.Contracts
    .AnyAsync(c => c.Student.CitizenId == dto.CitizenId
                && c.Status == "Active");

        if (hasActiveContract)
            throw new BadRequestException("Sinh viên đang có hợp đồng còn hiệu lực.");

        // Kiểm tra email đã được đăng ký trong hệ thống chưa (Users hoặc Đơn Pending)
        var emailExistsInUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExistsInUser)
            throw new BadRequestException("Email này đã được sử dụng trong hệ thống.");

        var emailExistsInRegistration = await _context.Registrations.AnyAsync(r => r.Email == dto.Email && (r.Status == "Pending" || r.Status == "Approved"));
        if (emailExistsInRegistration)
            throw new BadRequestException("Email này đã có đơn đăng ký đang chờ hoặc đã được duyệt.");

        // Kiểm tra phòng tồn tại không
        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new BadRequestException("Phòng không tồn tại.");

        // Kiểm tra phòng còn chỗ không
        if (room.CurrentOccupancy >= room.Capacity)
            throw new BadRequestException("Phòng đã đầy, vui lòng chọn phòng khác.");

        // Kiểm tra giới tính phù hợp không
        if (room.Building.GenderAllowed.ToLower() != dto.Gender.Trim().ToLower())
            throw new BadRequestException($"Phòng chỉ dành cho {room.Building.GenderAllowed}");

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
            RelativeName = dto.RelativeName,
            RelativePhone = dto.RelativePhone,
            Relationship = dto.Relationship,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = "Pending"
        };
        await _registrationRepo.AddAsync(registration);
        await _registrationRepo.SaveChangesAsync();

        return (true, "Đăng ký thành công! Vui lòng chờ admin duyệt đơn.", new RegistrationResponse
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

    public async Task<List<RegistrationResponse>> GetAllAsync()
    {
        var list = await _registrationRepo.GetAllAsync();
        return list.Select(r => new RegistrationResponse
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
    public async Task<List<RegistrationResponse>> GetPendingAsync()
    {
        var list = await _registrationRepo.GetPendingAsync();
        return list.Select(r => new RegistrationResponse
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

    public async Task<(bool Success, string Message)> ApproveAsync(int id, ApproveRegistrationRequest dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        { 
        // Lấy đơn đăng ký
        var registration = await _registrationRepo.GetByIdAsync(id);
        if (registration == null)
            throw new BadRequestException("Không tìm thấy đơn đăng ký.");

        if (registration.Status != "Pending")
            throw new BadRequestException("Đơn này đã được xử lý rồi.");

        if (dto.IsApproved)
        {
            // Cập nhật số người trong phòng
            var room = await _roomRepo.GetByIdAsync(registration.RoomId);
            if (room == null)
                throw new BadRequestException("Phòng không tồn tại.");

            if (room.CurrentOccupancy >= room.Capacity)
                throw new BadRequestException("Phòng đã đầy, không thể duyệt.");

            // Duyệt đơn
            registration.Status = "Approved";

            // 1. Tạo User (username = Email, password = CCCD)
            var user = new User
            {
                Email = registration.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registration.CitizenId),
                Role = "Student",
                IsActive = true,
                MustChangePassword = true
            };
            await _context.Users.AddAsync(user);

            // 2. Tạo Student
            var student = new Student
            {
                User = user,
                FullName = registration.FullName,
                CitizenId = registration.CitizenId,
                Gender = registration.Gender,
                Phone = registration.Phone,
                Email = registration.Email,
                PermanentAddress = registration.PermanentAddress
            };
            await _context.Students.AddAsync(student);

                // 3. Tạo thông tin Thân nhân
                var relative = new Relative
            {
                Student = student,
                FullName = registration.RelativeName,
                Phone = registration.RelativePhone,
                Relationship = registration.Relationship
            };
            await _context.Relatives.AddAsync(relative);

            // 4. Liên kết Student vào Đơn & Cập nhật phòng
            registration.Student = student;
            room.CurrentOccupancy += 1;
            if (room.CurrentOccupancy >= room.Capacity)
                room.Status = "Full";

            _context.Rooms.Update(room);

            // 5. Tạo hợp đồng
            var contract = new Contract
            {
                ContractCode = $"HD_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Student = student,
                RoomId = registration.RoomId,
                StartDate = registration.StartDate,
                EndDate = registration.EndDate,
                Status = "Active",
                Price = room.Price
            };
            await _context.Contracts.AddAsync(contract);

            // TODO: Gửi email thông báo tài khoản cho sinh viên (làm sau)
            // await _emailService.SendAccountInfoAsync(registration.Email, registration.FullName, registration.CitizenId);
        }
        else
        {
            // Từ chối đơn
            if (string.IsNullOrEmpty(dto.RejectionReason))
                    throw new BadRequestException("Vui lòng nhập lý do từ chối.");

                registration.Status = "Rejected";
            registration.RejectionReason = dto.RejectionReason;
        }

        _context.Registrations.Update(registration);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

            var message = dto.IsApproved
            ? "Duyệt đơn thành công, hợp đồng đã được tạo."
            : "Đã từ chối đơn đăng ký.";

            return (true, message);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

}