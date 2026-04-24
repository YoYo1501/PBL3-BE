namespace BackendAPI.Models.DTOs.Student.Requests;

public class UpdateStudentDto
{
    public string Phone { get; set; } = string.Empty;
    public string PermanentAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}