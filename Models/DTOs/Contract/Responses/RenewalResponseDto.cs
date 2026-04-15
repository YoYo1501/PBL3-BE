namespace BackendAPI.Models.DTOs.Contract.Responses;

public class RenewalResponseDto
{
    public int Id { get; set; }
    public string ContractCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}