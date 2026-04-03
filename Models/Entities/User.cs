namespace BackendAPI.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty; // Dành cho Admin 
        public string Phone { get; set; } = string.Empty;    // Dành cho Admin
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Student"; // "Student" | "Admin"
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Student? Student { get; set; }
        public ICollection<Notification> Notifications { get; set; } = [];

    }
}
