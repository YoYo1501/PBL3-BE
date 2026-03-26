namespace BackendAPI.Models.Entities
{

    public class Contract
    {
        public int Id { get; set; }
        public string ContractCode { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active";
        public decimal Price { get; set; }

        public Student Student { get; set; } = null!;
        public Room Room { get; set; } = null!;
    }
}
