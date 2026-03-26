namespace BackendAPI.Models.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CitizenId { get; set; } = string.Empty; // CCCD
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Registration> Registrations { get; set; } = [];
        public ICollection<Contract> Contracts { get; set; } = [];
        public ICollection<ViolationRecord> ViolationRecords { get; set; } = [];
        public ICollection<RoomTransferRequest> RoomTransferRequests { get; set; } = [];
        public ICollection<Invoice> Invoices { get; set; } = [];

    }
}
