namespace BackendAPI.Models.Entities;

public class RenewalRequest
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public int RenewalPackageId { get; set; }
    public RenewalPackages RenewalPackage { get; set; } = null!;
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string? RejectionReason { get; set; }
}