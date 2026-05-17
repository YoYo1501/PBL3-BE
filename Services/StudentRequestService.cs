using BackendAPI.Helpers;
using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.StudentRequest.Requests;
using BackendAPI.Models.DTOs.StudentRequest.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace BackendAPI.Services;

public class StudentRequestService(
    IStudentRequestRepository _repo,
    IContractRepository _contractRepo,
    IFacilityRepository _facilityRepo,
    INotificationService _notificationService) : IStudentRequestService
{
    private static readonly string[] CheckoutDateFormats = ["yyyy-MM-dd", "d/M/yyyy", "dd/MM/yyyy"];

    private StudentRequestResponseDto ToDto(StudentRequest r) => new()
    {
        Id = r.Id,
        StudentId = r.StudentId,
        StudentName = r.Student != null ? r.Student.FullName : "",
        RoomCode = r.Student?.Contracts
            .FirstOrDefault(c => c.Status == "Active" && c.Room != null)
            ?.Room?.RoomCode ?? "",
        RequestType = r.RequestType,
        Title = r.Title,
        Description = r.Description,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        ResolvedAt = r.ResolvedAt,
        ResolutionNote = r.ResolutionNote
    };

    public async Task<(bool Success, string Message, StudentRequestResponseDto? Data)> CreateRequestAsync(int studentId, CreateStudentRequestDto dto)
    {
        var activeContract = await _contractRepo.GetActiveContractAsync(studentId);
        if (activeContract == null)
            throw new BadRequestException("Bạn phải có hợp đồng đang hiệu lực mới có thể gửi yêu cầu.");

        var requestType = (dto.RequestType ?? string.Empty).Trim();
        if (string.Equals(requestType, "Checkout", StringComparison.OrdinalIgnoreCase))
        {
            var hasPendingCheckout = await _repo.HasPendingRequestAsync(studentId, "Checkout");
            if (hasPendingCheckout)
                throw new BadRequestException("Bạn đang có yêu cầu trả phòng đang chờ duyệt. Vui lòng chờ xử lý hoặc hủy yêu cầu hiện tại trước khi tạo đơn mới.");

            var checkoutDate = ResolveCheckoutDate(dto);
            if (!checkoutDate.HasValue)
                throw new BadRequestException("Vui lòng chọn ngày dự kiến trả phòng.");

            var today = VietnamTime.FromUtc(DateTime.UtcNow).Date;
            if (checkoutDate.Value.Date < today)
                throw new BadRequestException("Ngày dự kiến trả phòng không được ở trong quá khứ.");

            requestType = "Checkout";
        }

        var request = new StudentRequest
        {
            StudentId = studentId,
            RequestType = requestType,
            Title = dto.Title,
            Description = dto.Description,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(request);
        await _repo.SaveChangesAsync();

        // Lấy thông tin sinh viên để hiển thị trong thông báo
        var createdRequest = await _repo.GetByIdAsync(request.Id);
        var studentName = createdRequest?.Student?.FullName ?? $"Sinh viên #{studentId}";

        await _notificationService.CreateForAdminsAsync(
            "Yêu cầu sinh viên mới",
            $"Sinh viên {studentName} vừa gửi yêu cầu '{dto.Title}' thuộc loại {requestType}."
        );

        return (true, "Gửi yêu cầu thành công", ToDto(createdRequest ?? request));
    }

    private static DateTime? ResolveCheckoutDate(CreateStudentRequestDto dto)
    {
        if (dto.CheckoutDate.HasValue)
            return dto.CheckoutDate.Value.Date;

        const string prefix = "Ngày dự kiến trả phòng:";
        foreach (var line in (dto.Description ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            var rawDate = trimmed[prefix.Length..].Trim();
            if (DateTime.TryParseExact(rawDate, CheckoutDateFormats, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out var parsed))
                return parsed.Date;

            if (DateTime.TryParse(rawDate, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out parsed))
                return parsed.Date;
        }

        return null;
    }

    public async Task<(bool Success, string Message)> CancelRequestAsync(int studentId, int requestId)
    {
        var request = await _repo.GetByIdAsync(requestId);
        if (request == null)
            throw new BadRequestException("Không tìm thấy yêu cầu.");

        if (request.StudentId != studentId)
            throw new BadRequestException("Bạn không có quyền thao tác trên yêu cầu này.");

        if (request.Status != "Pending")
            throw new BadRequestException("Chỉ có thể hủy yêu cầu khi đang ở trạng thái chờ duyệt (Pending).");

        request.Status = "Cancelled";
        request.ResolvedAt = DateTime.UtcNow;
        request.ResolutionNote = "Người dùng tự hủy yêu cầu.";

        _repo.Update(request);
        await _repo.SaveChangesAsync();

        return (true, "Hủy yêu cầu thành công.");
    }

    public async Task<(bool Success, string Message)> UpdateRequestStatusAsync(int id, UpdateRequestStatusDto dto)
    {
        var request = await _repo.GetByIdAsync(id);
        if (request == null)
            throw new BadRequestException("Không tìm thấy yêu cầu.");

        var nextStatus = (dto.Status ?? string.Empty).Trim();
        var isValidTransition = (request.Status, nextStatus) switch
        {
            ("Pending", "Approved") => true,
            ("Pending", "Rejected") => true,
            ("Approved", "InProgress") when request.RequestType == "Maintenance" => true,
            ("InProgress", "Completed") when request.RequestType == "Maintenance" => true,
            _ => false
        };

        if (!isValidTransition)
            throw new BadRequestException("Trang thai yeu cau khong hop le hoac khong dung thu tu xu ly.");
        request.Status = nextStatus;
        if (!string.IsNullOrEmpty(dto.ResolutionNote))
            request.ResolutionNote = dto.ResolutionNote;

        if (nextStatus is "Rejected" or "Completed")
            request.ResolvedAt = DateTime.UtcNow;
        else if (nextStatus == "Approved" && request.ResolvedAt == null)
            request.ResolvedAt = DateTime.UtcNow;

        if (request.RequestType == "Checkout" && nextStatus == "Approved")
        {
            // Nếu duyệt yêu cầu trả phòng thì thanh lý hợp đồng đang hiệu lực.
            var contract = await _contractRepo.GetActiveContractAsync(request.StudentId);
            if (contract != null)
            {
                if (contract.Room != null)
                {
                    contract.Room.CurrentOccupancy = Math.Max(0, contract.Room.CurrentOccupancy - 1);
                    if (contract.Room.Status == "Full")
                        contract.Room.Status = "Available";
                }
                contract.Status = "Terminated";
                contract.EndDate = DateTime.UtcNow;
                await _contractRepo.UpdateContractAsync(contract);
            }
        }

        await SyncMaintenanceFacilityStatusAsync(request, nextStatus);

        _repo.Update(request);
        await _repo.SaveChangesAsync();

        return (true, "Cập nhật trạng thái yêu cầu thành công.");
    }

    private async Task SyncMaintenanceFacilityStatusAsync(StudentRequest request, string nextStatus)
    {
        if (request.RequestType != "Maintenance")
            return;

        var facilityStatus = nextStatus switch
        {
            "Approved" => "Damaged",
            "InProgress" => "UnderMaintenance",
            "Completed" => "Good",
            _ => null
        };

        if (facilityStatus == null)
            return;

        var facility = await FindFacilityForMaintenanceRequestAsync(request);
        if (facility == null)
            throw new BadRequestException("Khong tim thay thiet bi trong phong de cap nhat tinh trang.");

        facility.Status = facilityStatus;
        _facilityRepo.Update(facility);
    }

    private async Task<Facility?> FindFacilityForMaintenanceRequestAsync(StudentRequest request)
    {
        var roomId = request.Student?.Contracts
            .FirstOrDefault(c => c.Status == "Active")
            ?.RoomId;

        if (!roomId.HasValue)
            return null;

        var requestedFacilityName = ExtractFacilityName(request.Title);
        if (string.IsNullOrWhiteSpace(requestedFacilityName))
            return null;

        var normalizedRequestedName = NormalizeFacilityName(requestedFacilityName);
        var facilities = await _facilityRepo.GetByRoomIdAsync(roomId.Value);

        return facilities.FirstOrDefault(f =>
            NormalizeFacilityName(f.Name) == normalizedRequestedName);
    }

    private static string ExtractFacilityName(string title)
    {
        var safeTitle = title ?? string.Empty;
        var colonIndex = safeTitle.IndexOf(':');
        return (colonIndex >= 0 ? safeTitle[..colonIndex] : safeTitle).Trim();
    }

    private static string NormalizeFacilityName(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            if (!char.IsWhiteSpace(ch))
                builder.Append(ch == '\u0111' ? 'd' : ch);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public async Task<List<StudentRequestResponseDto>> GetAllRequestsAsync(string? status, string? requestType)
    {
        var list = await _repo.GetAllAsync(status, requestType);
        return list.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<StudentRequestResponseDto>> GetPagedRequestsAsync(StudentRequestListQueryDto query)
    {
        var page = query.GetPage();
        var pageSize = query.GetPageSize(5);
        var (items, totalCount) = await _repo.GetPagedAsync(query.Status, query.RequestType, page, pageSize);

        return new PagedResultDto<StudentRequestResponseDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };
    }

    public async Task<List<StudentRequestResponseDto>> GetMyRequestsAsync(int studentId, string? status)
    {
        var list = await _repo.GetByStudentIdAsync(studentId, status);
        return list.Select(ToDto).ToList();
    }
}
