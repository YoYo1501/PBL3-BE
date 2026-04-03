namespace BackendAPI.Models.DTOs.RoomTransfer.Requests
{
    public class ApproveTransfer
    {
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}
