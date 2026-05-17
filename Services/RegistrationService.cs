using BackendAPI.Data;
using BackendAPI.Models.DTOs.Registration.Requests;
using BackendAPI.Models.DTOs.Registration.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BackendAPI.Exceptions;
using BackendAPI.Helpers;
using BackendAPI.Models.DTOs.Common;

namespace BackendAPI.Services;

public class RegistrationService(
IRegistrationRepository _registrationRepo,
IRoomRepository _roomRepo,
IRoomTransferRepository _roomTransferRepo,
IEmailService _emailService,
INotificationService _notificationService,
AppDbContext _context) : IRegistrationService
{
    public async Task<(bool Success, string Message, DateTime? ExpiresAtUtc)> HoldRoomAsync(RegistrationRoomHoldRequest dto)
    {
        var holdToken = dto.HoldToken.Trim();
        if (string.IsNullOrWhiteSpace(holdToken))
            return (false, "Phiên giữ chỗ không hợp lệ.", null);

        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null)
            return (false, "Phòng không tồn tại.", null);

        if (room.Status != "Available")
            return (false, "Phòng hiện không khả dụng.", null);

        if (room.CurrentOccupancy >= room.Capacity)
            return (false, "Phòng đã đầy, vui lòng chọn phòng khác.", null);

        var pendingInRoom =
            await _registrationRepo.CountPendingByRoomAsync(room.Id) +
            await _roomTransferRepo.CountPendingToRoomAsync(room.Id);
        var (success, expiresAtUtc) = RegistrationRoomHoldStore.TryHold(
            room.Id,
            holdToken,
            room.CurrentOccupancy + pendingInRoom,
            room.Capacity);

        if (!success)
            return (false, "Phòng đang được giữ chỗ bởi người khác. Vui lòng chọn phòng khác hoặc thử lại sau 10 phút.", null);

        return (true, "Giữ chỗ thành công! Bạn có 10 phút để gửi đơn đăng ký.", expiresAtUtc);
    }

    public async Task<(bool Success, string Message, RegistrationResponse? Data)> RegisterAsync(RegistrationRequestDto dto)
    {
        dto.Email = dto.Email.Trim();
        dto.FullName = dto.FullName.Trim();
        dto.Phone = dto.Phone.Trim();
        dto.CitizenId = dto.CitizenId.Trim();
        dto.Email = dto.Email.ToLower();
        if (!dto.Email.EndsWith("@gmail.com"))
            throw new BadRequestException("Email phải có định dạng @gmail.com");

        // Kiểm tra CCCD đã có đơn chưa
        var hasPending = await _registrationRepo.HasPendingRegistrationAsync(dto.CitizenId);
        if (hasPending)
            throw new BadRequestException("CCCD này đã có đơn đăng ký.");
            
        var hasActiveContract = await _context.Contracts
    .AnyAsync(c => c.Student.CitizenId == dto.CitizenId
                && c.Status == "Active");

        if (hasActiveContract)
            throw new BadRequestException("Sinh viên đang có hợp đồng còn hiệu lực.");

        // Kiểm tra email đã được đăng ký trong hệ thống chưa (Users hoặc Đơn Pending)
        var emailExistsInOtherUser = await _context.Users
            .AnyAsync(u => u.Email == dto.Email && u.CitizenId != dto.CitizenId);
        if (emailExistsInOtherUser)
            throw new BadRequestException("Email này đã được sử dụng trong hệ thống.");

        var emailExistsInRegistration = await _context.Registrations
            .AnyAsync(r => r.Email == dto.Email && r.Status == "Pending");
        if (emailExistsInRegistration)
            throw new BadRequestException("Email này đã có đơn đăng ký đang chờ duyệt.");

        // Kiểm tra phòng tồn tại không
        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new BadRequestException("Phòng không tồn tại.");
        if (room.Status != "Available")
            throw new BadRequestException("Phòng hiện không khả dụng.");

        var roomHoldToken = dto.RoomHoldToken?.Trim() ?? string.Empty;
        if (!RegistrationRoomHoldStore.HasActiveHold(roomHoldToken, dto.RoomId))
            throw new BadRequestException("Phiên giữ chỗ đã hết hạn. Vui lòng chọn lại phòng.");

        // Kiểm tra phòng còn chỗ không
        var activeOtherHolds = RegistrationRoomHoldStore.CountActiveHolds(room.Id, roomHoldToken);
        var pendingInRoom =
            await _registrationRepo.CountPendingByRoomAsync(room.Id) +
            await _roomTransferRepo.CountPendingToRoomAsync(room.Id);
        if (room.CurrentOccupancy + pendingInRoom + activeOtherHolds >= room.Capacity)
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
        RegistrationRoomHoldStore.Release(roomHoldToken);
        await _notificationService.CreateForAdminsAsync(
            "Đơn đăng ký ở trú mới",
            $"{dto.FullName} vừa gửi đơn đăng ký vào phòng {room.RoomCode}."
        );

        return (true, "Đăng ký thành công! Vui lòng chờ admin duyệt đơn.", new RegistrationResponse
        {
            Id = registration.Id,
            RegistrationCode = registration.RegistrationCode,
            FullName = dto.FullName,
            CitizenId = dto.CitizenId,
            Phone = dto.Phone,
            Gender = dto.Gender,
            Email = dto.Email,
            RoomCode = room.RoomCode,
            Status = registration.Status,
            StartDate = registration.StartDate,
            EndDate = registration.EndDate,
            SubmittedAt = registration.SubmittedAt
        });
    }

    public async Task<(bool Success, string Message, RegistrationResponse? Data)> RegisterReturningAsync(int userId, ReturningRegistrationRequestDto dto)
    {
        var student = await _context.Students
            .Include(s => s.Relatives)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            throw new BadRequestException("Không tìm thấy thông tin sinh viên.");

        var hasPending = await _registrationRepo.HasPendingRegistrationAsync(student.CitizenId);
        if (hasPending)
            throw new BadRequestException("Bạn đã có đơn đăng ký đang chờ duyệt.");

        var hasActiveContract = await _context.Contracts
            .AnyAsync(c => c.StudentId == student.Id && c.Status == "Active");
        if (hasActiveContract)
            throw new BadRequestException("Bạn đang có hợp đồng còn hiệu lực.");

        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null)
            throw new BadRequestException("Phòng không tồn tại.");
        if (room.Status != "Available")
            throw new BadRequestException("Phòng hiện không khả dụng.");

        var roomHoldToken = dto.RoomHoldToken?.Trim() ?? string.Empty;
        if (!RegistrationRoomHoldStore.HasActiveHold(roomHoldToken, dto.RoomId))
            throw new BadRequestException("Phiên giữ chỗ đã hết hạn. Vui lòng chọn lại phòng.");

        var activeOtherHolds = RegistrationRoomHoldStore.CountActiveHolds(room.Id, roomHoldToken);
        var pendingInRoom =
            await _registrationRepo.CountPendingByRoomAsync(room.Id) +
            await _roomTransferRepo.CountPendingToRoomAsync(room.Id);
        if (room.CurrentOccupancy + pendingInRoom + activeOtherHolds >= room.Capacity)
            throw new BadRequestException("Phòng đã đầy, vui lòng chọn phòng khác.");

        if (room.Building.GenderAllowed.ToLower() != student.Gender.Trim().ToLower())
            throw new BadRequestException($"Phòng chỉ dành cho {room.Building.GenderAllowed}");

        var relative = student.Relatives.FirstOrDefault();
        var registration = new Registration
        {
            RegistrationCode = $"REG_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString()[..6].ToUpper()}",
            StudentId = student.Id,
            RoomId = dto.RoomId,
            FullName = student.FullName,
            CitizenId = student.CitizenId,
            Gender = student.Gender,
            Phone = student.Phone,
            Email = student.Email,
            PermanentAddress = student.PermanentAddress,
            RelativeName = relative?.FullName ?? string.Empty,
            RelativePhone = relative?.Phone ?? string.Empty,
            Relationship = relative?.Relationship ?? string.Empty,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = "Pending"
        };

        await _registrationRepo.AddAsync(registration);
        await _registrationRepo.SaveChangesAsync();
        RegistrationRoomHoldStore.Release(roomHoldToken);
        await _notificationService.CreateForAdminsAsync(
            "Đơn đăng ký ở lại",
            $"{student.FullName} vừa gửi đơn đăng ký ở lại vào phòng {room.RoomCode}."
        );

        return (true, "Đăng ký ở lại thành công! Vui lòng chờ admin duyệt đơn.", new RegistrationResponse
        {
            Id = registration.Id,
            RegistrationCode = registration.RegistrationCode,
            FullName = student.FullName,
            CitizenId = student.CitizenId,
            Phone = student.Phone,
            Gender = student.Gender,
            Email = student.Email,
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
            CitizenId = r.CitizenId,
            Phone = r.Phone,
            Gender = r.Gender,
            Email = r.Email,
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
            CitizenId = r.CitizenId,
            Phone = r.Phone,
            Gender = r.Gender,
            Email = r.Email,
            RoomCode = r.Room.RoomCode,
            Status = r.Status,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            SubmittedAt = r.SubmittedAt
        }).ToList();
    }

    public async Task<PagedResultDto<RegistrationResponse>> GetPagedPendingAsync(RegistrationListQueryDto query)
    {
        var page = query.GetPage();
        var pageSize = query.GetPageSize(5);
        var (items, totalCount) = await _registrationRepo.GetPagedPendingAsync(page, pageSize);

        return new PagedResultDto<RegistrationResponse>
        {
            Items = items.Select(r => new RegistrationResponse
            {
                Id = r.Id,
                RegistrationCode = r.RegistrationCode,
                FullName = r.FullName,
                CitizenId = r.CitizenId,
                Phone = r.Phone,
                Gender = r.Gender,
                Email = r.Email,
                RoomCode = r.Room.RoomCode,
                Status = r.Status,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                SubmittedAt = r.SubmittedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };
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

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.CitizenId == registration.CitizenId);

            if (student == null)
            {
                // 1. Tạo User (username = CCCD, password = CCCD)
                var user = new User
                {
                    Email = registration.Email,
                    CitizenId = registration.CitizenId,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registration.CitizenId),
                    Role = "Student",
                    IsActive = true,
                    MustChangePassword = true
                };
                await _context.Users.AddAsync(user);

                // 2. Tạo Student
                student = new Student
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
            }
            else
            {
                var hasActiveContract = await _context.Contracts
                    .AnyAsync(c => c.StudentId == student.Id && c.Status == "Active");
                if (hasActiveContract)
                    throw new BadRequestException("Sinh viên đang có hợp đồng còn hiệu lực.");

                student.FullName = registration.FullName;
                student.Gender = registration.Gender;
                student.Phone = registration.Phone;
                student.Email = registration.Email;
                student.PermanentAddress = registration.PermanentAddress;
                if (student.User != null)
                {
                    student.User.Email = registration.Email;
                    student.User.IsActive = true;
                    _context.Users.Update(student.User);
                }
                _context.Students.Update(student);
            }

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

            if (student.User?.MustChangePassword == true)
            {
                // Gửi email thông báo tài khoản cho sinh viên mới
                await _emailService.SendAccountInfoAsync(registration.Email, registration.FullName, registration.CitizenId);
            }
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
