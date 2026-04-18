namespace BackendAPI.Models.DTOs.Ocr;

public class CccdInformationDto
{
    public string IdNumber { get; set; } = string.Empty;
    public string CitizenId
    {
        get => string.IsNullOrWhiteSpace(IdNumber) ? string.Empty : IdNumber;
        set => IdNumber = value;
    }
    public string FullName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string HomeTown { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
}
