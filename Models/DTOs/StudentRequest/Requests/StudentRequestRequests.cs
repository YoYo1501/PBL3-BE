namespace BackendAPI.Models.DTOs.StudentRequest.Requests;

public class CreateStudentRequestDto
{
    public string RequestType { get; set; } = string.Empty; // "Checkout", "Maintenance", "Other"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? CheckoutDate { get; set; }
}

public class UpdateRequestStatusDto
{
    public string Status { get; set; } = string.Empty; // "Approved", "Rejected", "InProgress", "Completed"
    public string? ResolutionNote { get; set; }
}
