namespace BackendAPI.Models.DTOs.Contract.Responses;

public class RenewalPackageResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public DateTime NewEndDate { get; set; }
    public decimal EstimatedPrice { get; set; }
}