using BackendAPI.Models.DTOs.Contract.Requests;
using BackendAPI.Models.DTOs.Contract.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class ContractService(IContractRepository _repo) : IContractService
{
    public async Task<(bool Success, string Message, ContractResponseDto? Data)> GetMyContractAsync(int studentId)
    {
        var contract = await _repo.GetActiveContractAsync(studentId);
        if (contract == null)
            return (false, "Bạn không có hợp đồng lưu trú hiệu lực.", null);

        var daysRemaining = (contract.EndDate - DateTime.UtcNow).Days;

        return (true, "Lấy thông tin hợp đồng thành công.", new ContractResponseDto
        {
            Id = contract.Id,
            ContractCode = contract.ContractCode,
            RoomCode = contract.Room.RoomCode,
            RoomType = contract.Room.RoomType,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Price = contract.Price,
            Status = contract.Status,
            DaysRemaining = daysRemaining,
            CanRenew = daysRemaining <= 30
        });
    }

    public async Task<(bool Success, string Message, List<RenewalPackageResponseDto>? Packages)> GetRenewalPackagesAsync(int studentId)
    {
        var contract = await _repo.GetActiveContractAsync(studentId);
        if (contract == null)
            return (false, "Bạn không có hợp đồng lưu trú hiệu lực.", null);

        var daysRemaining = (contract.EndDate - DateTime.UtcNow).Days;
        if (daysRemaining > 30)
            return (false, "Hợp đồng chưa đến thời hạn gia hạn (còn hơn 30 ngày).", null);

        var hasUnpaid = await _repo.HasUnpaidInvoiceAsync(studentId);
        if (hasUnpaid)
            return (false, "Bạn đang có hóa đơn chưa thanh toán.", null);

        var violations = await _repo.CountViolationsAsync(studentId);
        if (violations >= 3)
            return (false, "Bạn có từ 3 lần vi phạm trở lên, không đủ điều kiện gia hạn.", null);

        var packages = await _repo.GetActivePackagesAsync();
        var result = packages.Select(p => new RenewalPackageResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            DurationMonths = p.DurationMonths,
            NewEndDate = contract.EndDate.AddMonths(p.DurationMonths),
            EstimatedPrice = contract.Price * p.DurationMonths
        }).ToList();

        return (true, "Lấy danh sách gói gia hạn thành công.", result);
    }

    public async Task<(bool Success, string Message)> SubmitRenewalAsync(int studentId, RenewalRequestDto dto)
    {
        var existing = await _repo.GetPendingRenewalAsync(studentId);
        if (existing != null)
            return (false, "Bạn đã có yêu cầu gia hạn đang chờ duyệt.");

        var contract = await _repo.GetActiveContractAsync(studentId);
        if (contract == null)
            return (false, "Không có hợp đồng hiệu lực.");

        var request = new RenewalRequest
        {
            StudentId = studentId,
            ContractId = contract.Id,
            RenewalPackageId = dto.RenewalPackageId,
            Status = "Pending"
        };

        await _repo.AddRenewalRequestAsync(request);
        await _repo.SaveChangesAsync();

        return (true, "Gửi yêu cầu gia hạn thành công! Vui lòng chờ admin duyệt.");
    }

    public async Task<List<RenewalResponseDto>> GetAllPendingRenewalsAsync()
    {
        var list = await _repo.GetAllPendingRenewalsAsync();
        return list.Select(r => new RenewalResponseDto
        {
            Id = r.Id,
            ContractCode = r.Contract.ContractCode,
            PackageName = r.RenewalPackage.Name,
            Status = r.Status,
            RequestedAt = r.RequestedAt
        }).ToList();
    }

    public async Task<(bool Success, string Message)> ApproveRenewalAsync(int requestId, ApproveRenewalDto dto)
    {
        var request = await _repo.GetRenewalByIdAsync(requestId);
        if (request == null)
            return (false, "Không tìm thấy yêu cầu gia hạn.");

        if (request.Status != "Pending")
            return (false, "Yêu cầu này đã được xử lý rồi.");

        if (dto.IsApproved)
        {
            request.Contract.EndDate = request.Contract.EndDate
                .AddMonths(request.RenewalPackage.DurationMonths);
            request.Status = "Approved";
            await _repo.UpdateContractAsync(request.Contract);
        }
        else
        {
            if (string.IsNullOrEmpty(dto.RejectionReason))
                return (false, "Vui lòng nhập lý do từ chối.");

            request.Status = "Rejected";
            request.RejectionReason = dto.RejectionReason;
        }

        await _repo.UpdateRenewalAsync(request);
        await _repo.SaveChangesAsync();

        return (true, dto.IsApproved ? "Duyệt gia hạn thành công." : "Đã từ chối yêu cầu gia hạn.");
    }

    public async Task<List<ContractResponseDto>> GetAllContractsAsync()
    {
        var contracts = await _repo.GetAllContractsAsync();
        return contracts.Select(c => new ContractResponseDto
        {
            Id = c.Id,
            ContractCode = c.ContractCode,
            RoomCode = c.Room?.RoomCode ?? "",
            RoomType = c.Room?.RoomType ?? "",
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Price = c.Price,
            Status = c.Status,
            DaysRemaining = (c.EndDate - DateTime.UtcNow).Days >= 0 ? (c.EndDate - DateTime.UtcNow).Days : 0,
            CanRenew = (c.EndDate - DateTime.UtcNow).Days <= 30 && c.Status == "Active",
            StudentId = c.StudentId,
            StudentName = c.Student?.FullName
        }).OrderByDescending(c => c.StartDate).ToList();
    }

    public async Task<ContractResponseDto?> GetContractByIdAsync(int id)
    {
        var c = await _repo.GetContractByIdAsync(id);
        if (c == null) return null;

        return new ContractResponseDto
        {
            Id = c.Id,
            ContractCode = c.ContractCode,
            RoomCode = c.Room?.RoomCode ?? "",
            RoomType = c.Room?.RoomType ?? "",
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Price = c.Price,
            Status = c.Status,
            DaysRemaining = (c.EndDate - DateTime.UtcNow).Days >= 0 ? (c.EndDate - DateTime.UtcNow).Days : 0,
            CanRenew = (c.EndDate - DateTime.UtcNow).Days <= 30 && c.Status == "Active",
            StudentId = c.StudentId,
            StudentName = c.Student?.FullName
        };
    }

    public async Task<(bool Success, string Message)> UpdateContractAsync(int id, UpdateContractRequestDto dto)
    {
        var contract = await _repo.GetContractByIdAsync(id);
        if (contract == null)
            return (false, "Hợp đồng không tồn tại.");

        if (dto.StartDate.HasValue) contract.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) contract.EndDate = dto.EndDate.Value;
        if (dto.Price.HasValue) contract.Price = dto.Price.Value;
        if (!string.IsNullOrEmpty(dto.Status)) contract.Status = dto.Status;

        await _repo.UpdateContractAsync(contract);
        await _repo.SaveChangesAsync();

        return (true, "Cập nhật hợp đồng thành công.");
    }

    public async Task<(bool Success, string Message)> DeleteContractAsync(int id)
    {
        var contract = await _repo.GetContractByIdAsync(id);
        if (contract == null)
            return (false, "Hợp đồng không tồn tại.");

        contract.Status = "Inactive";
        // Alternatively, maybe add a Repo method to actually delete if needed
        // but soft delete by Status is usually safer
        await _repo.UpdateContractAsync(contract);
        await _repo.SaveChangesAsync();

        return (true, "Vô hiệu hóa hợp đồng thành công.");
    }
}