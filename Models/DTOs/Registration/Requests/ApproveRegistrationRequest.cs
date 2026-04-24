using System.ComponentModel;

namespace BackendAPI.Models.DTOs.Registration.Requests;

public class ApproveRegistrationRequest
{
    [DefaultValue(true)]
    public bool IsApproved { get; set; }

    [DefaultValue("Hồ sơ hợp lệ")]
    public string? RejectionReason { get; set; }
}