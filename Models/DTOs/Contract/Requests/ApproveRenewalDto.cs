namespace BackendAPI.Models.DTOs.Contract.Requests;

public class ApproveRenewalDto
{
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}