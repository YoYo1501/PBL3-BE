namespace BackendAPI.Models.Entities;

public class Relative
{
    public int Id { get; set; }
    public int? StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty; // "B?", "M?", "Anh/ch?"

    // Navigation
    public Student? Student { get; set; }
}
