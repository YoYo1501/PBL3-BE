namespace BackendAPI.Models.Entities;

public class RoomTransferRequest
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int FromRoomId { get; set; }
    public int ToRoomId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected
    public string? RejectionReason { get; set; }
    public int TransferCountInSemester { get; set; } = 0;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public int SemesterId { get; set; }
    public SemesterPeriods Semester { get; set; } = null!;
    // Navigation
    public Student Student { get; set; } = null!;
    public Room FromRoom { get; set; } = null!;
    public Room ToRoom { get; set; } = null!;
}