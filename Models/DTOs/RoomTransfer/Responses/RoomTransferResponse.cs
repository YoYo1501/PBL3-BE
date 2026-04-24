namespace BackendAPI.Models.DTOs.RoomTransfer;

public class RoomTransferResponseDto
{
    public int Id { get; set; }
    public string FromRoomCode { get; set; } = string.Empty;
    public string ToRoomCode { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime RequestedAt { get; set; }
}