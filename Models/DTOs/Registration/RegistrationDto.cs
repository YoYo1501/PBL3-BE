using System.ComponentModel;

namespace BackendAPI.Models.DTOs.Registration;

public class RegistrationRequestDto
{
    [DefaultValue("Trần Văn B")]
    public string FullName { get; set; } = string.Empty;

    [DefaultValue("048201002345")]
    public string CitizenId { get; set; } = string.Empty;

    [DefaultValue("Nam")]
    public string Gender { get; set; } = string.Empty;

    [DefaultValue("0987654321")]
    public string Phone { get; set; } = string.Empty;

    [DefaultValue("studentb@gmail.com")]
    public string Email { get; set; } = string.Empty;

    [DefaultValue("456 Đường XYZ, Hà Nội")]
    public string PermanentAddress { get; set; } = string.Empty;

    [DefaultValue(1)]
    public int RoomId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}