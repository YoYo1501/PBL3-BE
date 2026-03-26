using System.ComponentModel;

namespace BackendAPI.Models.DTOs.Registration;

public class ApproveRegistrationDto
{
    [DefaultValue(true)]
    public bool IsApproved { get; set; }

    [DefaultValue("Hồ sơ hợp lệ")]
    public string? RejectionReason { get; set; }
}