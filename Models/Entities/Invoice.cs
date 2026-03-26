namespace BackendAPI.Models.Entities
{

    public class Invoice
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int RoomId { get; set; }
        public string Period { get; set; } = string.Empty;
        public decimal RoomFee { get; set; }
        public decimal ElectricFee { get; set; }
        public decimal WaterFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Unpaid";
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public Student Student { get; set; } = null!;
        public Room Room { get; set; } = null!;
    }
}
