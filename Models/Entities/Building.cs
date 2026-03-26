namespace BackendAPI.Models.Entities
{
    public class Building
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;    // "A", "B"
        public string Name { get; set; } = string.Empty;
        public string GenderAllowed { get; set; } = string.Empty; // "Nam" | "Nữ"

        public ICollection<Room> Rooms { get; set; } = [];
    }
}
