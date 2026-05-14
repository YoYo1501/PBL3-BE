namespace BackendAPI.Models.DTOs.Contract.Responses;

public class RenewalResponseDto
{
    public int Id { get; set; }
    public string ContractCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractEndDateBeforeRenewal { get; set; }
    public DateTime ContractEndDateAfterRenewal { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string? RejectionReason { get; set; }
}
