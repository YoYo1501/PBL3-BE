namespace BackendAPI.Models.DTOs.Contract.Responses;

public class ContractResponseDto
{
    public int Id { get; set; }
    public string ContractCode { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
    public bool CanRenew { get; set; }
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
}