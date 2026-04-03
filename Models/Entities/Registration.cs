namespace BackendAPI.Models.Entities
{
    public class Registration
    {
        public int Id { get; set; }
        public string RegistrationCode { get; set; } = string.Empty; // "REG_2026_001"
        public int? StudentId { get; set; } // Nullable because student might not exist yet when guest registers
        public int RoomId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CitizenId { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        
        // Thân nhân
        public string RelativeName { get; set; } = string.Empty;
        public string RelativePhone { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending" | "Approved" | "Rejected"
        public string? RejectionReason { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Student? Student { get; set; }
        public Room Room { get; set; } = null!;
    }
}
