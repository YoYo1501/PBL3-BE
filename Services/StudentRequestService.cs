using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.StudentRequest.Requests;
using BackendAPI.Models.DTOs.StudentRequest.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class StudentRequestService(IStudentRequestRepository _repo, IContractRepository _contractRepo) : IStudentRequestService
{
    private StudentRequestResponseDto ToDto(StudentRequest r) => new()
    {
        Id = r.Id,
        StudentId = r.StudentId,
        StudentName = r.Student != null ? r.Student.FullName : "",
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

        return (true, "Gửi yêu cầu thành công", ToDto(request));
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

        request.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.ResolutionNote))
            request.ResolutionNote = dto.ResolutionNote;

        if (dto.Status == "Completed" || dto.Status == "Rejected" || dto.Status == "Approved")
        {
            request.ResolvedAt = DateTime.UtcNow;
        }

        if (request.RequestType == "Checkout" && dto.Status == "Completed")
        {
            // Nếu là yêu cầu trả phòng và đã hoàn thành -> Thanh lý hợp đồng
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

    public async Task<List<StudentRequestResponseDto>> GetMyRequestsAsync(int studentId, string? status)
    {
        var list = await _repo.GetByStudentIdAsync(studentId, status);
        return list.Select(ToDto).ToList();
    }
}