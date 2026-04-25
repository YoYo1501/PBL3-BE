using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.StudentRequest.Requests;
using BackendAPI.Models.DTOs.StudentRequest.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class StudentRequestService(IStudentRequestRepository _repo, IContractRepository _contractRepo, INotificationService _notificationService) : IStudentRequestService
{
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

        var request = new StudentRequest
        {
            StudentId = studentId,
            RequestType = dto.RequestType,
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
            "Yeu cau sinh vien moi",
            $"Sinh vien {studentName} vua gui yeu cau '{dto.Title}' thuoc loai {dto.RequestType}."
        );

        return (true, "Gửi yêu cầu thành công", ToDto(createdRequest ?? request));
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
        if (nextStatus != "Approved" && nextStatus != "Rejected")
            throw new BadRequestException("Trạng thái yêu cầu chỉ được duyệt hoặc từ chối.");

        if (request.Status != "Pending")
            throw new BadRequestException("Chỉ có thể xử lý yêu cầu đang chờ duyệt.");

        request.Status = nextStatus;
        if (!string.IsNullOrEmpty(dto.ResolutionNote))
            request.ResolutionNote = dto.ResolutionNote;

        request.ResolvedAt = DateTime.UtcNow;

        if (request.RequestType == "Checkout" && nextStatus == "Approved")
        {
            // Nếu duyệt yêu cầu trả phòng thì thanh lý hợp đồng đang hiệu lực.
            var contract = await _contractRepo.GetActiveContractAsync(request.StudentId);
            if (contract != null)
            {
                contract.Status = "Terminated";
                contract.EndDate = DateTime.UtcNow;
                await _contractRepo.UpdateContractAsync(contract);
            }
        }

        _repo.Update(request);
        await _repo.SaveChangesAsync();

        return (true, "Cập nhật trạng thái yêu cầu thành công.");
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
