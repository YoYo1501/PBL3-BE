namespace BackendAPI.Models.Entities
{
    public class RenewalPackages
    {
   
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty; // "1 kỳ", "1 năm"
            public int DurationMonths { get; set; }           // 6, 12
            public bool IsActive { get; set; } = true;
        
    }
}
