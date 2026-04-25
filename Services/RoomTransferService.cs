using BackendAPI.Data;
using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Room;
using BackendAPI.Models.DTOs.RoomTransfer;
using BackendAPI.Models.DTOs.RoomTransfer.Requests;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace BackendAPI.Services;

public class RoomTransferService(IRoomTransferRepository _repo, IMemoryCache _cache, AppDbContext _context, INotificationService _notificationService) : IRoomTransferService
{
    public async Task<(bool Success, string Message, List<RoomDto>? Rooms)> GetAvailableRoomsAsync(int studentId)
    {
        // Kiểm tra hợp đồng hiệu lực
        var contract = await _repo.GetActiveContractAsync(studentId);
        if (contract == null)
            return (false, "Bạn không có hợp đồng lưu trú hiệu lực.", null);

        // Kiểm tra số lần chuyển trong kỳ
        var semester = await _repo.GetCurrentSemesterAsync();
        if (semester == null)
            return (false, "Không tìm thấy học kỳ hiện tại.", null);

        var transferCount = await _repo.CountTransferInSemesterAsync(studentId, semester.Id);
        if (transferCount >= 1)
            return (false, "Bạn đã chuyển phòng trong học kỳ này. Mỗi sinh viên chỉ được chuyển 1 lần/kỳ.", null);

        // Lấy phòng hiện tại
        var currentRoom = await _repo.GetRoomByIdAsync(contract.RoomId);
        if (currentRoom == null)
            return (false, "Không tìm thấy phòng hiện tại.", null);

        // Lấy danh sách phòng trống cùng giới tính
        var rooms = await _repo.GetAvailableRoomsAsync(currentRoom.Building.GenderAllowed, currentRoom.Id);

        var result = rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            RoomCode = r.RoomCode,
            RoomType = r.RoomType,
            Capacity = r.Capacity,
            CurrentOccupancy = r.CurrentOccupancy,
            Status = r.Status,
            BuildingCode = r.Building?.Code ?? string.Empty,
            BuildingName = r.Building?.Name ?? string.Empty,
            GenderAllowed = r.Building?.GenderAllowed ?? string.Empty
        }).ToList();

        return (true, "Lấy danh sách phòng thành công.", result);
    }

    public async Task<(bool Success, string Message)> HoldRoomAsync(int studentId, HoldRoomRequest dto)
    {
        var cacheKey = $"hold_room_{dto.ToRoomId}";

        // Kiểm tra phòng đã bị giữ chỗ chưa
        if (_cache.TryGetValue(cacheKey, out int holdingStudentId) && holdingStudentId != studentId)
            return (false, "Phòng đang được giữ chỗ bởi sinh viên khác, vui lòng chọn phòng khác.");

        // Kiểm tra phòng còn chỗ không
        var room = await _repo.GetRoomByIdAsync(dto.ToRoomId);
        if (room == null)
            return (false, "Phòng không tồn tại.");
        if (room.CurrentOccupancy >= room.Capacity)
            return (false, "Phòng đã đầy.");

        // Giữ chỗ 10 phút
        _cache.Set(cacheKey, studentId, TimeSpan.FromMinutes(10));

        return (true, "Giữ chỗ thành công! Bạn có 10 phút để xác nhận chuyển phòng.");
    }

    public async Task<(bool Success, string Message)> SubmitTransferAsync(int studentId, BackendAPI.Models.DTOs.RoomTransfer.Requests.RoomTransferRequest dto)
    {
        if (string.IsNullOrEmpty(dto.Reason) || dto.Reason.Length < 15)
            return (false, "Lý do chuyển phòng phải có ít nhất 15 ký tự.");

        // Kiểm tra còn giữ chỗ không
        var cacheKey = $"hold_room_{dto.ToRoomId}";
        if (!_cache.TryGetValue(cacheKey, out int holdingStudentId) || holdingStudentId != studentId)
            return (false, "Hết thời gian giữ chỗ, vui lòng thực hiện lại.");

        // Lấy hợp đồng hiện tại
        var contract = await _repo.GetActiveContractAsync(studentId);
        if (contract == null)
            return (false, "Không có hợp đồng hiệu lực.");

        var semester = await _repo.GetCurrentSemesterAsync();
        if (semester == null)
            return (false, "Không tìm thấy học kỳ hiện tại.");

        // Tạo yêu cầu chuyển phòng
        var request = new BackendAPI.Models.Entities.RoomTransferRequest
        {
            StudentId = studentId,
            FromRoomId = contract.RoomId,
            ToRoomId = dto.ToRoomId,
            Reason = dto.Reason,
            Status = "Pending",
            SemesterId = semester.Id
        };

        await _repo.AddAsync(request);
        await _repo.SaveChangesAsync();

        // Lấy thông tin sinh viên và phòng để hiển thị trong thông báo
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        var toRoom = await _repo.GetRoomByIdAsync(dto.ToRoomId);
        var studentName = student?.FullName ?? $"Sinh viên #{studentId}";
        var toRoomCode = toRoom?.RoomCode ?? $"#{dto.ToRoomId}";

        await _notificationService.CreateForAdminsAsync(
            "Yeu cau chuyen phong moi",
            $"Sinh vien {studentName} vua gui yeu cau chuyen phong sang phong {toRoomCode}."
        );

        // Xóa cache giữ chỗ
        _cache.Remove(cacheKey);

        return (true, "Gửi yêu cầu chuyển phòng thành công! Vui lòng chờ admin duyệt.");
    }

    public async Task<(bool Success, string Message)> ApproveTransferAsync(int requestId, bool isApproved, string? rejectionReason)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var transfer = await _repo.GetPendingTransferByIdAsync(requestId);
        if (transfer == null)
            return (false, "Không tìm thấy yêu cầu chuyển phòng.");

        if (isApproved)
        {
            var contract = await _repo.GetActiveContractAsync(transfer.StudentId);
            if (contract == null)
                return (false, "Sinh vien khong con hop dong hieu luc de chuyen phong.");

            if (contract.RoomId != transfer.FromRoomId)
                return (false, "Phong hien tai cua sinh vien khong khop voi yeu cau chuyen phong.");
            // Giảm người ở phòng cũ
            var fromRoom = await _repo.GetRoomByIdAsync(transfer.FromRoomId);
            if (fromRoom == null)
                return (false, "Phong cu khong ton tai.");

            if (fromRoom.CurrentOccupancy <= 0)
                return (false, "Du lieu phong cu khong hop le de duyet chuyen phong.");

            // Tăng người ở phòng mới
            var toRoom = await _repo.GetRoomByIdAsync(transfer.ToRoomId);
            if (toRoom == null || toRoom.CurrentOccupancy >= toRoom.Capacity)
                return (false, "Phòng mới đã đầy hoặc không tồn tại.");

            fromRoom.CurrentOccupancy -= 1;
            if (fromRoom.Status == "Full")
                fromRoom.Status = "Available";
            await _repo.UpdateRoomAsync(fromRoom);

            toRoom.CurrentOccupancy += 1;
            if (toRoom.CurrentOccupancy >= toRoom.Capacity) toRoom.Status = "Full";
            await _repo.UpdateRoomAsync(toRoom);

            // Cập nhật hợp đồng
            contract.RoomId = transfer.ToRoomId;
            await _repo.UpdateContractAsync(contract);

            transfer.Status = "Approved";
        }
        else
        {
            if (string.IsNullOrEmpty(rejectionReason))
                return (false, "Vui lòng nhập lý do từ chối.");

            transfer.Status = "Rejected";
            transfer.RejectionReason = rejectionReason;
        }

        await _repo.UpdateTransferAsync(transfer);
        await _repo.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, isApproved ? "Duyệt chuyển phòng thành công." : "Đã từ chối yêu cầu chuyển phòng.");
    }

    public async Task<List<RoomTransferResponseDto>> GetAllPendingAsync()
    {
        var list = await _repo.GetAllPendingAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<RoomTransferResponseDto>> GetPagedPendingAsync(RoomTransferPendingQueryDto query)
    {
        var page = query.GetPage();
        var pageSize = query.GetPageSize(5);
        var (items, totalCount) = await _repo.GetPagedPendingAsync(page, pageSize);

        return new PagedResultDto<RoomTransferResponseDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };
    }

    public async Task<(bool Success, string Message)> CancelTransferAsync(int requestId, int studentId)
    {
        var transfer = await _repo.GetTransferByIdAsync(requestId);
        if (transfer == null)
            return (false, "Không tìm thấy yêu cầu chuyển phòng.");

        if (transfer.StudentId != studentId)
            return (false, "Bạn không có quyền hủy yêu cầu này.");

        if (transfer.Status != "Pending")
            return (false, "Chỉ có thể hủy yêu cầu đang chờ duyệt.");

        transfer.Status = "Cancelled";
        await _repo.UpdateTransferAsync(transfer);
        await _repo.SaveChangesAsync();

        return (true, "Đã hủy yêu cầu chuyển phòng.");
    }

    public async Task<List<RoomTransferResponseDto>> GetMyTransfersAsync(int studentId)
    {
        var list = await _repo.GetMyTransfersAsync(studentId);
        return list.Select(r => new RoomTransferResponseDto
        {
            Id = r.Id,
            FromRoomCode = r.FromRoom?.RoomCode ?? "",
            ToRoomCode = r.ToRoom?.RoomCode ?? "",
            Reason = r.Reason,
            Status = r.Status,
            RejectionReason = r.RejectionReason,
            RequestedAt = r.RequestedAt
        }).ToList();
    }

    private static RoomTransferResponseDto ToDto(BackendAPI.Models.Entities.RoomTransferRequest request)
    {
        return new RoomTransferResponseDto
        {
            Id = request.Id,
            FromRoomCode = request.FromRoom.RoomCode,
            ToRoomCode = request.ToRoom.RoomCode,
            Reason = request.Reason,
            Status = request.Status,
            RejectionReason = request.RejectionReason,
            RequestedAt = request.RequestedAt
        };
    }
}
