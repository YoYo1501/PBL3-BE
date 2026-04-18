using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Contract.Requests;
using BackendAPI.Models.DTOs.Contract.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class ContractService(IContractRepository repo) : IContractService
{
    public async Task<(bool Success, string Message, ContractResponseDto? Data)> GetMyContractAsync(int studentId)
    {
        var contract = await repo.GetActiveContractAsync(studentId);
        if (contract == null)
            return (false, "Bạn không có hợp đồng lưu trú hiệu lực.", null);

        return (true, "Lấy thông tin hợp đồng thành công.", ToDto(contract));
    }

    public async Task<(bool Success, string Message, List<RenewalPackageResponseDto>? Packages)> GetRenewalPackagesAsync(int studentId)
    {
        var contract = await repo.GetActiveContractAsync(studentId);
        if (contract == null)
            return (false, "Bạn không có hợp đồng lưu trú hiệu lực.", null);

        var daysRemaining = (contract.EndDate - DateTime.UtcNow).Days;
        if (daysRemaining > 30)
            return (false, "Hợp đồng chưa đến thời hạn gia hạn (còn hơn 30 ngày).", null);

        var hasUnpaid = await repo.HasUnpaidInvoiceAsync(studentId);
        if (hasUnpaid)
            return (false, "Bạn đang có hóa đơn chưa thanh toán.", null);

        var violations = await repo.CountViolationsAsync(studentId);
        if (violations >= 3)
            return (false, "Bạn có từ 3 lần vi phạm trở lên, không đủ điều kiện gia hạn.", null);

        var packages = await repo.GetActivePackagesAsync();
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
        var existing = await repo.GetPendingRenewalAsync(studentId);
        if (existing != null)
            return (false, "Bạn đã có yêu cầu gia hạn đang chờ duyệt.");

        var contract = await repo.GetActiveContractAsync(studentId);
        if (contract == null)
            return (false, "Không có hợp đồng hiệu lực.");

        var request = new RenewalRequest
        {
            StudentId = studentId,
            ContractId = contract.Id,
            RenewalPackageId = dto.RenewalPackageId,
            Status = "Pending"
        };

        await repo.AddRenewalRequestAsync(request);
        await repo.SaveChangesAsync();

        return (true, "Gửi yêu cầu gia hạn thành công! Vui lòng chờ admin duyệt.");
    }

    public async Task<List<RenewalResponseDto>> GetAllPendingRenewalsAsync()
    {
        var list = await repo.GetAllPendingRenewalsAsync();
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
        var request = await repo.GetRenewalByIdAsync(requestId);
        if (request == null)
            return (false, "Không tìm thấy yêu cầu gia hạn.");

        if (request.Status != "Pending")
            return (false, "Yêu cầu này đã được xử lý rồi.");

        if (dto.IsApproved)
        {
            request.Contract.EndDate = request.Contract.EndDate.AddMonths(request.RenewalPackage.DurationMonths);
            request.Status = "Approved";
            await repo.UpdateContractAsync(request.Contract);
        }
        else
        {
            if (string.IsNullOrEmpty(dto.RejectionReason))
                return (false, "Vui lòng nhập lý do từ chối.");

            request.Status = "Rejected";
            request.RejectionReason = dto.RejectionReason;
        }

        await repo.UpdateRenewalAsync(request);
        await repo.SaveChangesAsync();

        return (true, dto.IsApproved ? "Duyệt gia hạn thành công." : "Đã từ chối yêu cầu gia hạn.");
    }

    public async Task<List<ContractResponseDto>> GetAllContractsAsync()
    {
        var contracts = await repo.GetAllContractsAsync();
        return contracts.Select(ToDto).OrderByDescending(c => c.StartDate).ToList();
    }

    public async Task<PagedResultDto<ContractResponseDto>> GetPagedContractsAsync(ContractListQueryDto query)
    {
        var page = query.GetPage();
        var pageSize = query.GetPageSize(6);
        var (items, totalCount) = await repo.GetPagedContractsAsync(query.Keyword, query.Status, page, pageSize);

        return new PagedResultDto<ContractResponseDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };
    }

    public async Task<ContractResponseDto?> GetContractByIdAsync(int id)
    {
        var contract = await repo.GetContractByIdAsync(id);
        return contract == null ? null : ToDto(contract);
    }

    public async Task<(bool Success, string Message)> UpdateContractAsync(int id, UpdateContractRequestDto dto)
    {
        var contract = await repo.GetContractByIdAsync(id);
        if (contract == null)
            return (false, "Hợp đồng không tồn tại.");

        if (dto.StartDate.HasValue) contract.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) contract.EndDate = dto.EndDate.Value;
        if (dto.Price.HasValue) contract.Price = dto.Price.Value;
        if (!string.IsNullOrEmpty(dto.Status)) contract.Status = dto.Status;

        await repo.UpdateContractAsync(contract);
        await repo.SaveChangesAsync();

        return (true, "Cập nhật hợp đồng thành công.");
    }

    public async Task<(bool Success, string Message)> DeleteContractAsync(int id)
    {
        var contract = await repo.GetContractByIdAsync(id);
        if (contract == null)
            return (false, "Hợp đồng không tồn tại.");

        contract.Status = "Inactive";
        await repo.UpdateContractAsync(contract);
        await repo.SaveChangesAsync();

        return (true, "Vô hiệu hóa hợp đồng thành công.");
    }

    private static ContractResponseDto ToDto(Contract c)
    {
        var daysRemaining = (c.EndDate - DateTime.UtcNow).Days;
        var normalizedDays = daysRemaining >= 0 ? daysRemaining : 0;

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
            DaysRemaining = normalizedDays,
            CanRenew = daysRemaining <= 30 && c.Status == "Active",
            StudentId = c.StudentId,
            StudentName = c.Student?.FullName
        };
    }
}
