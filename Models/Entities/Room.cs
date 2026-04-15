
namespace BackendAPI.Models.Entities
{
    public class Room : ISoftDelete
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int BuildingId { get; set; }
        public string RoomCode { get; set; } = string.Empty;  // "A101"
        public string RoomType { get; set; } = string.Empty;  // "4 người" | "6 người"
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; } = 0;
        public string Status { get; set; } = "Available"; // "Available" | "Full" | "Locked"
        public decimal Price { get; set; }// Giá thuê phòng hàng tháng

        // Navigation
        public Building Building { get; set; } = null!;
        public ICollection<RoomTransferRequest> TransferRequestsFrom { get; set; } = [];
        public ICollection<RoomTransferRequest> TransferRequestsTo { get; set; } = [];
        public ICollection<ElectricWaterReading> ElectricWaterReadings { get; set; } = [];

    }
}
