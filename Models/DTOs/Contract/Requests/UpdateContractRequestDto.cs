using System;

namespace BackendAPI.Models.DTOs.Contract.Requests;

public class UpdateContractRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Price { get; set; }
    public string? Status { get; set; }
}
