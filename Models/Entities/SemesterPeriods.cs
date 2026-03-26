namespace BackendAPI.Models.Entities
{
    public class SemesterPeriods
    {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty; // "HK1 2025-2026"
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsRegistrationOpen { get; set; } = false;
        
    }
}
