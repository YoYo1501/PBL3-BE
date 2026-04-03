namespace BackendAPI.Models.DTOs.RoomTransfer.Requests
{
    public class RoomTransferRequest
    {
        public int ToRoomId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
